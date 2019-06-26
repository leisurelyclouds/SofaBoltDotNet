using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;

namespace Remoting.Codecs
{
    /// <summary>
    /// This class mainly hack the <seealso cref="io.netty.handler.codec.ByteToMessageDecoder"/> to provide batch submission capability.
    /// This can be used the same way as ByteToMessageDecoder except the case your following inbound handler may get a decoded msg,
    /// which actually is an array list, then you can submit the list of msgs to an executor to process. For example
    /// <pre>
    ///   if (msg instanceof List<object>) {
    ///       processorManager.getDefaultExecutor().execute(new Runnable() {
    ///           public void run() {
    ///               // batch submit to an executor
    ///               for (Object m : (List<object><?>) msg) {
    ///                   RpcCommandHandler.this.process(ctx, m);
    ///               }
    ///           }
    ///       });
    ///   } else {
    ///       process(ctx, msg);
    ///   }
    /// </pre>
    /// You can check the method <seealso cref="AbstractBatchDecoder#channelRead(IChannelHandlerContext, Object)"/> ()}
    ///   to know the detail modification.
    /// </summary>
    public abstract class AbstractBatchDecoder : ChannelHandlerAdapter
    {
        /// <summary>
        /// Cumulate <seealso cref="IByteBuffer"/>s by merge them into one <seealso cref="IByteBuffer"/>'s, using memory copies.
        /// </summary>
        public static readonly Cumulator MERGE_CUMULATOR = new CumulatorAnonymousInnerClass();

        private class CumulatorAnonymousInnerClass : Cumulator
        {
            public CumulatorAnonymousInnerClass()
            {
            }

            public virtual IByteBuffer cumulate(IByteBufferAllocator alloc, IByteBuffer cumulation, IByteBuffer @in)
            {
                IByteBuffer buffer;
                if (cumulation.WriterIndex > cumulation.MaxCapacity - @in.ReadableBytes || cumulation.ReferenceCount > 1)
                {
                    // Expand cumulation (by replace it) when either there is not more room in the buffer
                    // or if the refCnt is greater then 1 which may happen when the user use slice().retain() or
                    // duplicate().retain().
                    //
                    // See:
                    // - https://github.com/netty/netty/issues/2327
                    // - https://github.com/netty/netty/issues/1764
                    buffer = expandCumulation(alloc, cumulation, @in.ReadableBytes);
                }
                else
                {
                    buffer = cumulation;
                }
                buffer.WriteBytes(@in);
                @in.Release();
                return buffer;
            }
        }

        /// <summary>
        /// Cumulate <seealso cref="IByteBuffer"/>s by add them to a <seealso cref="CompositeByteBuffer"/> and so do no memory copy whenever possible.
        /// Be aware that <seealso cref="CompositeByteBuffer"/> use a more complex indexing implementation so depending on your use-case
        /// and the decoder implementation this may be slower then just use the <seealso cref="#MERGE_CUMULATOR"/>.
        /// </summary>
        public static readonly Cumulator COMPOSITE_CUMULATOR = new CumulatorAnonymousInnerClass2();

        private class CumulatorAnonymousInnerClass2 : Cumulator
        {
            public CumulatorAnonymousInnerClass2()
            {
            }

            public virtual IByteBuffer cumulate(IByteBufferAllocator alloc, IByteBuffer cumulation, IByteBuffer @in)
            {
                IByteBuffer buffer;
                if (cumulation.ReferenceCount > 1)
                {
                    // Expand cumulation (by replace it) when the refCnt is greater then 1 which may happen when the user
                    // use slice().retain() or duplicate().retain().
                    //
                    // See:
                    // - https://github.com/netty/netty/issues/2327
                    // - https://github.com/netty/netty/issues/1764
                    buffer = expandCumulation(alloc, cumulation, @in.ReadableBytes);
                    buffer.WriteBytes(@in);
                    @in.Release();
                }
                else
                {
                    CompositeByteBuffer composite;
                    if (cumulation is CompositeByteBuffer)
                    {
                        composite = (CompositeByteBuffer)cumulation;
                    }
                    else
                    {
                        int readable = cumulation.ReadableBytes;
                        composite = alloc.CompositeBuffer();
                        composite.AddComponent(cumulation).SetWriterIndex(readable);
                    }
                    composite.AddComponent(@in).SetWriterIndex(composite.WriterIndex + @in.ReadableBytes);
                    buffer = composite;
                }
                return buffer;
            }
        }

        internal IByteBuffer cumulation;
        private Cumulator cumulator = MERGE_CUMULATOR;
        private bool singleDecode;
        private bool decodeWasNull;
        private bool first;
        private int discardAfterReads = 16;
        private int numReads;

        /// <summary>
        /// If set then only one message is decoded on each <seealso cref="#channelRead(IChannelHandlerContext, Object)"/>
        /// call. This may be useful if you need to do some protocol upgrade and want to make sure nothing is mixed up.
        /// 
        /// Default is {@code false} as this has performance impacts.
        /// </summary>
        public virtual bool SingleDecode
        {
            set
            {
                singleDecode = value;
            }
            get
            {
                return singleDecode;
            }
        }


        /// <summary>
        /// Set the <seealso cref="Cumulator"/> to use for cumulate the received <seealso cref="IByteBuffer"/>s.
        /// </summary>
        public virtual void setCumulator(Cumulator cumulator)
        {
            this.cumulator = cumulator ?? throw new NullReferenceException("cumulator");
        }

        /// <summary>
        /// Set the number of reads after which <seealso cref="IByteBuffer#discardSomeReadBytes()"/> are called and so free up memory.
        /// The default is {@code 16}.
        /// </summary>
        public virtual int DiscardAfterReads
        {
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("discardAfterReads must be > 0");
                }
                discardAfterReads = value;
            }
        }

        /// <summary>
        /// Returns the actual number of readable bytes in the internal cumulative
        /// buffer of this decoder. You usually do not need to rely on this value
        /// to write a decoder. Use it only when you must use it at your own risk.
        /// This method is a shortcut to <seealso cref="#internalBuffer() internalBuffer().ReadableBytes"/>.
        /// </summary>
        protected internal virtual int actualReadableBytes()
        {
            return internalBuffer().ReadableBytes;
        }

        /// <summary>
        /// Returns the internal cumulative buffer of this decoder. You usually
        /// do not need to access the internal buffer directly to write a decoder.
        /// Use it only when you must use it at your own risk.
        /// </summary>
        protected internal virtual IByteBuffer internalBuffer()
        {
            if (cumulation != null)
            {
                return cumulation;
            }
            else
            {
                return Unpooled.Empty;
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public final void handlerRemoved(io.netty.channel.IChannelHandlerContext ctx) throws Exception
        public override void HandlerRemoved(IChannelHandlerContext ctx)
        {
            IByteBuffer buf = internalBuffer();
            int readable = buf.ReadableBytes;
            if (readable > 0)
            {
                IByteBuffer bytes = buf.ReadBytes(readable);
                buf.Release();
                ctx.FireChannelRead(bytes);
            }
            else
            {
                buf.Release();
            }
            cumulation = null;
            numReads = 0;
            ctx.FireChannelReadComplete();
            handlerRemoved0(ctx);
        }

        /// <summary>
        /// Gets called after the <seealso cref="ByteToMessageDecoder"/> was removed from the actual context and it doesn't handle
        /// events anymore.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: protected void handlerRemoved0(io.netty.channel.IChannelHandlerContext ctx) throws Exception
        protected internal virtual void handlerRemoved0(IChannelHandlerContext ctx)
        {
        }

        /// <summary>
        /// This method has been modified to check the size of decoded msgs, which is represented by the
        /// local variable {@code RecyclableArrayList out}. If has decoded more than one msg,
        /// then construct an array list to submit all decoded msgs to the pipeline.
        /// </summary>
        /// <param name="ctx"> channel handler context </param>
        /// <param name="msg"> data </param>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void channelRead(io.netty.channel.IChannelHandlerContext ctx, Object msg) throws Exception
        public override void ChannelRead(IChannelHandlerContext ctx, object msg)
        {
            if (msg is IByteBuffer)
            {
                var @out = new List<object>();
                try
                {
                    IByteBuffer data = (IByteBuffer)msg;
                    first = cumulation == null;
                    if (first)
                    {
                        cumulation = data;
                    }
                    else
                    {
                        cumulation = cumulator.cumulate(ctx.Allocator, cumulation, data);
                    }
                    callDecode(ctx, cumulation, @out);
                }
                catch (DecoderException)
                {
                    throw;
                }
                catch (Exception t)
                {
                    throw new DecoderException(t);
                }
                finally
                {
                    if (cumulation != null && !cumulation.IsReadable())
                    {
                        numReads = 0;
                        cumulation.Release();
                        cumulation = null;
                    }
                    else if (++numReads >= discardAfterReads)
                    {
                        // We did enough reads already try to discard some bytes so we not risk to see a OOME.
                        // See https://github.com/netty/netty/issues/4275
                        numReads = 0;
                        discardSomeReadBytes();
                    }

                    int size = @out.Count;
                    if (size == 0)
                    {
                        decodeWasNull = true;
                    }
                    else if (size == 1)
                    {
                        ctx.FireChannelRead(@out[0]);
                    }
                    else
                    {
                        List<object> ret = new List<object>(size);
                        for (int i = 0; i < size; i++)
                        {
                            ret.Add(@out[i]);
                        }
                        ctx.FireChannelRead(ret);
                    }

                    //@out.recycle();
                }
            }
            else
            {
                ctx.FireChannelRead(msg);
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void channelReadComplete(io.netty.channel.IChannelHandlerContext ctx) throws Exception
        public override void ChannelReadComplete(IChannelHandlerContext ctx)
        {
            numReads = 0;
            discardSomeReadBytes();
            if (decodeWasNull)
            {
                decodeWasNull = false;
                if (!ctx.Channel.Configuration.AutoRead)
                {
                    ctx.Read();
                }
            }
            ctx.FireChannelReadComplete();
        }

        protected internal void discardSomeReadBytes()
        {
            if (cumulation != null && !first && cumulation.ReferenceCount == 1)
            {
                // discard some bytes if possible to make more room in the
                // buffer but only if the refCnt == 1  as otherwise the user may have
                // used slice().retain() or duplicate().retain().
                //
                // See:
                // - https://github.com/netty/netty/issues/2327
                // - https://github.com/netty/netty/issues/1764
                cumulation.DiscardSomeReadBytes();
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: @Override public void channelInactive(io.netty.channel.IChannelHandlerContext ctx) throws Exception
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            var @out = new List<object>();
            try
            {
                if (cumulation != null)
                {
                    callDecode(ctx, cumulation, @out);
                    decodeLast(ctx, cumulation, @out);
                }
                else
                {
                    decodeLast(ctx, Unpooled.Empty, @out);
                }
            }
            catch (DecoderException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new DecoderException(e);
            }
            finally
            {
                try
                {
                    if (cumulation != null)
                    {
                        cumulation.Release();
                        cumulation = null;
                    }
                    int size = @out.Count;
                    for (int i = 0; i < size; i++)
                    {
                        ctx.FireChannelRead(@out[i]);
                    }
                    if (size > 0)
                    {
                        // Something was read, call fireChannelReadComplete()
                        ctx.FireChannelReadComplete();
                    }
                    ctx.FireChannelInactive();
                }
                finally
                {
                    // recycle in all cases
                    //@out.recycle();
                }
            }
        }

        /// <summary>
        /// Called once data should be decoded from the given <seealso cref="IByteBuffer"/>. This method will call
        /// <seealso cref="#decode(IChannelHandlerContext, IByteBuffer, List<object>)"/> as long as decoding should take place.
        /// </summary>
        /// <param name="ctx">           the <seealso cref="IChannelHandlerContext"/> which this <seealso cref="ByteToMessageDecoder"/> belongs to </param>
        /// <param name="in">            the <seealso cref="IByteBuffer"/> from which to read data </param>
        /// <param name="out">           the <seealso cref="List<object>"/> to which decoded messages should be added </param>
        protected internal virtual void callDecode(IChannelHandlerContext ctx, IByteBuffer @in, List<object> @out)
        {
            try
            {
                while (@in.IsReadable())
                {
                    int outSize = @out.Count;
                    int oldInputLength = @in.ReadableBytes;
                    decode(ctx, @in, @out);

                    // Check if this handler was removed before continuing the loop.
                    // If it was removed, it is not safe to continue to operate on the buffer.
                    //
                    // See https://github.com/netty/netty/issues/1664
                    if (ctx.Removed)
                    {
                        break;
                    }

                    if (outSize == @out.Count)
                    {
                        if (oldInputLength == @in.ReadableBytes)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (oldInputLength == @in.ReadableBytes)
                    {
                        throw new DecoderException(StringUtil.SimpleClassName(GetType()) + ".decode() did not read anything but decoded a message.");
                    }

                    if (SingleDecode)
                    {
                        break;
                    }
                }
            }
            catch (DecoderException)
            {
                throw;
            }
            catch (Exception cause)
            {
                throw new DecoderException(cause);
            }
        }

        /// <summary>
        /// Is called one last time when the <seealso cref="IChannelHandlerContext"/> goes in-active. Which means the
        /// <seealso cref="#channelInactive(IChannelHandlerContext)"/> was triggered.
        /// 
        /// By default this will just call <seealso cref="#decode(IChannelHandlerContext, IByteBuffer, List<object>)"/> but sub-classes may
        /// override this for some special cleanup operation.
        /// </summary>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: protected void decodeLast(io.netty.channel.IChannelHandlerContext ctx, io.netty.buffer.IByteBuffer in, java.util.List<object><Object> out) throws Exception
        protected internal virtual void decodeLast(IChannelHandlerContext ctx, IByteBuffer @in, List<object> @out)
        {
            decode(ctx, @in, @out);
        }

        internal static IByteBuffer expandCumulation(IByteBufferAllocator alloc, IByteBuffer cumulation, int readable)
        {
            IByteBuffer oldCumulation = cumulation;
            cumulation = alloc.Buffer(oldCumulation.ReadableBytes + readable);
            cumulation.WriteBytes(oldCumulation);
            oldCumulation.Release();
            return cumulation;
        }

        /// <summary>
        /// Cumulate <seealso cref="IByteBuffer"/>s.
        /// </summary>
        public interface Cumulator
        {
            /// <summary>
            /// Cumulate the given <seealso cref="IByteBuffer"/>s and return the <seealso cref="IByteBuffer"/> that holds the cumulated bytes.
            /// The implementation is responsible to correctly handle the life-cycle of the given <seealso cref="IByteBuffer"/>s and so
            /// call <seealso cref="IByteBuffer#release()"/> if a <seealso cref="IByteBuffer"/> is fully consumed.
            /// </summary>
            IByteBuffer cumulate(IByteBufferAllocator alloc, IByteBuffer cumulation, IByteBuffer @in);
        }

        /// <summary>
        /// Decode the from one <seealso cref="IByteBuffer"/> to an other. This method will be called till either the input
        /// <seealso cref="IByteBuffer"/> has nothing to read when return from this method or till nothing was read from the input
        /// <seealso cref="IByteBuffer"/>.
        /// </summary>
        /// <param name="ctx">           the <seealso cref="IChannelHandlerContext"/> which this <seealso cref="ByteToMessageDecoder"/> belongs to </param>
        /// <param name="in">            the <seealso cref="IByteBuffer"/> from which to read data </param>
        /// <param name="out">           the <seealso cref="List<object>"/> to which decoded messages should be added </param>
        /// <exception cref="Exception">    is thrown if an error accour </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: protected abstract void decode(io.netty.channel.IChannelHandlerContext ctx, io.netty.buffer.IByteBuffer in, java.util.List<object><Object> out) throws Exception;
        protected internal abstract void decode(IChannelHandlerContext ctx, IByteBuffer @in, List<object> @out);
    }


    //public class RecyclableArrayList : List<object>
    //{
    //    private static int DEFAULT_INITIAL_CAPACITY = 8;
    //    static readonly ThreadLocalPool<RecyclableArrayList> RECYCLER = new ThreadLocalPool<RecyclableArrayList>(handle => new RecyclableArrayList(handle));


    //    private bool _insertSinceRecycled;

    //    /**
    //     * Create a new empty {@link RecyclableArrayList} instance
    //     */
    //    public static RecyclableArrayList newInstance()
    //    {
    //        return newInstance(DEFAULT_INITIAL_CAPACITY);
    //    }

    //    /**
    //     * Create a new empty {@link RecyclableArrayList} instance with the given capacity.
    //     */
    //    public static RecyclableArrayList newInstance(int minCapacity)
    //    {
    //        RecyclableArrayList ret = RECYCLER.Take();
    //        ret.ensureCapacity(minCapacity);
    //        return ret;
    //    }

    //    private ThreadLocalPool.Handle handle;

    //    private RecyclableArrayList(ThreadLocalPool.Handle handle)
    //        : this(handle, DEFAULT_INITIAL_CAPACITY)
    //    {
    //    }

    //    private RecyclableArrayList(ThreadLocalPool.Handle handle, int initialCapacity)
    //       : base(initialCapacity)
    //    {
    //        this.handle = handle;
    //    }


    //    public override bool AddRange(Collection c)
    //    {
    //        checkNullElements(c);
    //        if (base.AddRange(c))
    //        {
    //            _insertSinceRecycled = true;
    //            return true;
    //        }
    //        return false;
    //    }

    //    public override bool AddRange(int index, Collection c)
    //    {
    //        checkNullElements(c);
    //        if (base.AddRange(index, c))
    //        {
    //            _insertSinceRecycled = true;
    //            return true;
    //        }
    //        return false;
    //    }

    //    private static void checkNullElements(Collection c)
    //    {
    //        if (c is RandomAccess && c is List<object>) {
    //            // produce less garbage
    //            List<object> list = (List<object>)c;
    //            int size = list.Count;
    //            for (int i = 0; i < size; i++)
    //            {
    //                if (list[i] == null)
    //                {
    //                    throw new ArgumentException("c contains null values");
    //                }
    //            }
    //        } else
    //        {
    //            var iterator = c.iterator();
    //            while (iterator.hasNext())
    //            {
    //                var element = iterator.next();
    //                if (element == null)
    //                {
    //                    throw new ArgumentException("c contains null values");
    //                }
    //            }
    //        }
    //    }


    //    public override bool Add(object element)
    //    {
    //        if (element == null)
    //        {
    //            throw new NullReferenceException("element");
    //        }
    //        if (base.Add(element))
    //        {
    //            _insertSinceRecycled = true;
    //            return true;
    //        }
    //        return false;
    //    }

    //    public override void Insert(int index, object element)
    //    {
    //        if (element == null)
    //        {
    //            throw new NullReferenceException("element");
    //        }
    //        base.Insert(index, element);
    //        _insertSinceRecycled = true;
    //    }

    //    public override object set(int index, object element)
    //    {
    //        if (element == null)
    //        {
    //            throw new NullReferenceException("element");
    //        }
    //        object old = base.(index, element);
    //        _insertSinceRecycled = true;
    //        return old;
    //    }

    //    /**
    //     * Returns {@code true} if any elements where added or set. This will be reset once {@link #recycle()} was called.
    //     */
    //    public bool insertSinceRecycled()
    //    {
    //        return _insertSinceRecycled;
    //    }

    //    /**
    //     * Clear and recycle this instance.
    //     */
    //    public bool recycle()
    //    {
    //        Clear();
    //        _insertSinceRecycled = false;
    //        handle.Release(this);
    //        return true;
    //    }
    //}
}
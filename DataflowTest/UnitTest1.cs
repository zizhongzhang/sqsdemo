using System.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace DataflowTest
{
    public interface ISqsClient
    {
        Task<Message> ReceiveMessage();
    }

    public class UnitTest1
    {

        [Fact]
        public void PipelineNormal()
        {
            var sourceBlock = new BatchBlock<int>(10);
            var transformBlock = new TransformBlock<int[], int>(numbers =>
            {
                Thread.Sleep(2000);
                return Task.FromResult(numbers.Sum());
            });
            var actionBlock = new ActionBlock<int>(num => Console.WriteLine($"Total: {num}"));

            sourceBlock.LinkTo(transformBlock, new DataflowLinkOptions { PropagateCompletion = true });
            transformBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            foreach (var num in Enumerable.Range(1, 10))
            {
                sourceBlock.Post(num);
            }

            sourceBlock.Complete();
            actionBlock.Completion.Wait();
        }

        [Fact]
        public void BatchBlockBehaviour()
        {
            var block = new BatchBlock<int>(10);
            foreach (var num in Enumerable.Range(1, 10))
            {
                block.Post(num);
            }

            block.Post(11);
            block.Complete();

            var messages = block.Receive();
            Assert.Equal(10, messages.Last());
            Assert.Equal(10, messages.Count());
            Assert.Equal(10, messages.Max());

            messages = block.Receive();
            Assert.Equal(1, messages.Count());
            Assert.Equal(11, messages.First());
        }

        [Fact]
        public async Task BufferBlockBehaviour()
        {
            var block = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 1 });
            await block.SendAsync(13);
            await block.SendAsync(14);

            Assert.True(block.Count == 1);

            var message = block.Receive();
            Assert.True(message.Equals(13));
            Assert.True(block.Count == 0);

        }

        [Fact]
        public async Task DoNotBlockSourceBlock()
        {
            var block = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 1 });
            await block.SendAsync(13);
            await block.SendAsync(14);

            Console.WriteLine("before blocking");
            block.Completion.Wait();
            Console.WriteLine("this line never reached");
        }
    }
}
using Microsoft.Azure.Cosmos;
using Polly;
using Polly.Retry;

namespace MassiveRocketAssignment.Utilities
{
    public class RetryManager
    {
        public static AsyncRetryPolicy WaitAndRetryPolicy
        { 
            get 
            {
                return Polly.Policy.Handle<AggregateException>()
                                   .Or<CosmosException>()
                                   .OrInner<Exception>()
                                   .WaitAndRetryAsync(
                                        RetryCount,
                                        retryAttempt => TimeSpan.FromMilliseconds(InitialWait * Math.Pow(2, retryAttempt)),
                                        (exception, time) => LogError(exception, time));
            
            } 
        }

        private static void LogError(Exception exception, TimeSpan time)
        {
            if (exception is CosmosException)
            {
                File.AppendAllText("D:\\Log-CosmosException.txt", $"Retrying after span: {time} because Received {exception.Message} and trace: ({exception.StackTrace}).");
            }
            else if (exception is AggregateException)
            {
                File.AppendAllText("D:\\Log-AggregateException.txt", $"Retrying after span: {time} because Received {exception.Message} and trace: ({exception.StackTrace}).");
            }
            else
            {
                File.AppendAllText("D:\\Log-Exception.txt", $"Retrying after span: {time} because Received {exception.Message} and trace: ({exception.StackTrace}).");
            }   
        }

        public static int RetryCount { get; set; } = 5;
        public static int InitialWait { get; set; } = 10;
    }
}

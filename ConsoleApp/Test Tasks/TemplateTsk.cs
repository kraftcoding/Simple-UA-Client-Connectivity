using Opc.Ua;
using SimpleUAClientLibrary.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simple_UA_Client_Connectivity.Tasks
{
    internal class TemplateTsk
    {
        public static async Task Launch(ProgramCtrl Prg, int msec)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            
            Task.Run(() =>
            {
                Console.WriteLine("Task is RUNNING");
                bool result = false;
                do
                {
                    Console.WriteLine("Enter 'true' to stop...");
                    bool.TryParse(Console.ReadLine(), out result);
                } while (!result);
                
                if(result) cancellationTokenSource.Cancel();
            });

            try
            {
                Utils.Trace("Starting long-running task...");
                await LongRunningTaskAsync(token, Prg, msec);
            }
            catch (OperationCanceledException)
            {
                Utils.Trace("Task was cancelled by user");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            Utils.Trace("Program completed");

        }

        static async Task LongRunningTaskAsync(CancellationToken token, ProgramCtrl Prg, int msec)
        {
            do
            {
                // Check if cancellation is requested
                token.ThrowIfCancellationRequested();
                
                // ... call here the sub-procedure
            }
            while (!token.IsCancellationRequested);

            Utils.Trace("Task completed successfully");
        }

        internal static void SubProcedure(ProgramCtrl Prg, string[] nodeIds, int msec)
        {

            //... here goes the logic of the sub-procedure

            Thread.Sleep(msec);
        }
    }
}

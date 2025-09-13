using Opc.Ua;
using SimpleUAClientLibrary.Controllers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple_UA_Client_Connectivity.Tasks
{
    internal class ReadNodesTsk
    {
        public static async Task Launch(ProgramManager Prg, string[] nodeIds, int msec)
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
                await LongRunningTaskAsync(token, Prg, nodeIds, msec);
            }
            catch (OperationCanceledException)
            {
                Utils.Trace("Task was cancelled by user");
            }
            catch (Exception ex)
            {
                Utils.Trace("Error: " + ex.ToString());
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            Utils.Trace("Program completed");

        }

        static async Task LongRunningTaskAsync(CancellationToken token, ProgramManager Prg, string[] nodeIds, int msec)
        {
            do
            {
                // Check if cancellation is requested
                token.ThrowIfCancellationRequested();
                ReadNodes(Prg, nodeIds, msec);
            }
            while (!token.IsCancellationRequested);

            Utils.Trace("Task completed successfully");
        }

        internal static void ReadNodes(ProgramManager Prg, string[] nodeIds, int msec)
        {
            List<NodeId> variableIds = new List<NodeId>();
            List<Type> expectedTypes = new List<Type>();

            List<object> values;
            List<ServiceResult> errors;

            foreach (var nodeId in nodeIds)
            {
                variableIds.Add(new NodeId(nodeId));
                expectedTypes.Add(null);
            }

            Prg.m_session.ReadValues(variableIds, expectedTypes, out values, out errors);

            int i = 0;
            foreach (object obj in values)
            {
                switch (obj)
                {
                    case null:
                        Utils.Trace(errors[i].ToLongString() + ", " + errors[i].Code + ", " + nodeIds[i]);
                        break;
                    default:
                        if (obj.GetType() == typeof(DateTime)) Utils.Trace(obj.ToString() + ", " + nodeIds[i]);
                        if (obj.GetType() == typeof(Opc.Ua.ServerStatusDataType))
                        {
                            Utils.Trace("State: " + ((Opc.Ua.ServerStatusDataType)values[0]).State + ", " + nodeIds[i]);
                        }
                        break;
                }
                i++;
            }

            Thread.Sleep(msec);
        }
    }
}

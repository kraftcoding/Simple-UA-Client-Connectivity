using Opc.Ua;
using SimpleUAClientLibrary.Controllers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple_UA_Client_Connectivity.Tasks
{
    internal class TestTaskCancellationTkn
    {
        public static async Task Launch(ProgramCtrl Prg, string[] nodeIds)
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
                await LongRunningTaskAsync(token, Prg, nodeIds);
            }
            catch (OperationCanceledException)
            {
                Utils.Trace("Task was cancelled by user");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            Utils.Trace("Program completed");

        }

        static async Task LongRunningTaskAsync(CancellationToken token, ProgramCtrl Prg, string[] nodeIds)
        {
            do
            {
                // Check if cancellation is requested
                token.ThrowIfCancellationRequested();
                LaunchThreads(Prg, nodeIds);
            }
            while (!token.IsCancellationRequested);

            Utils.Trace("Task completed successfully");
        }

        internal static void LaunchThreads(ProgramCtrl Prg, string[] nodeIds)
        {
            if (nodeIds.Length == 1)
            {
                NodeId dataNodeId = new NodeId(nodeIds[0]);
                DataValue simulatedDataValue = Prg.m_session.ReadValue(dataNodeId);
                Utils.Trace(simulatedDataValue.ToString());
            }
            else
            {
                List<NodeId> variableIds = new List<NodeId>();
                List<Type> expectedTypes = new List<Type>();

                List<object> values;
                List<ServiceResult> errors;

                foreach (var nedId in nodeIds)
                {
                    variableIds.Add(new NodeId(nedId));
                    expectedTypes.Add(null);
                }

                Prg.m_session.ReadValues(variableIds, expectedTypes, out values, out errors);

                foreach (object obj in values)
                {
                    if (obj == null) break;
                    if (obj.GetType() == typeof(DateTime)) Utils.Trace(obj.ToString());
                    if (obj.GetType() == typeof(Opc.Ua.ServerStatusDataType))
                    {
                        Utils.Trace("CurrentTime: " + ((Opc.Ua.ServerStatusDataType)values[0]).CurrentTime);
                        Utils.Trace("State: " + ((Opc.Ua.ServerStatusDataType)values[0]).State);
                    }
                }

                Thread.Sleep(2000);
            }
        }

    }
}

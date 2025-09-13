using Opc.Ua;
using Opc.Ua.Client;
using SimpleUAClientLibrary.Controllers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple_UA_Client_Connectivity.Tasks
{
    internal class WriteNodesTsk
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
                WriteNodes(Prg, nodeIds, msec);
            }
            while (!token.IsCancellationRequested);

            Utils.Trace("Task completed successfully");
        }

        internal static void WriteNodes(ProgramManager Prg, string[] nodeIds, int msec)
        {
            foreach (var nodeId in nodeIds)
            {
                WriteValue(Prg, new NodeId(nodeId), System.DateTime.Now);
            }

            Thread.Sleep(msec);
        }

        // Get type of variable in OPC Server which should be written and cast the value before actually writing it
        //public Task<bool> WriteValue(ProgramManager Prg, string nodeName, string varName, ushort namespaceIndex, string value)
        internal static void WriteValue(ProgramManager Prg, NodeId nodeId, DateTime value)
        {
            try
            {
                //NodeId nodeId = new NodeId($"{nodeName}.{varName}", namespaceIndex);

                // Read the node you want to write to
                var nodeToWrIteTo = Prg.m_session.ReadValue(nodeId);

                // Get type of the specific variable you want to write 
                BuiltInType type = nodeToWrIteTo.WrappedValue.TypeInfo.BuiltInType;

                // Get the corresponding C# datatype
                Type csType = Type.GetType($"System.{type}");

                // Cast the value
                var castedValue = Convert.ChangeType(value, csType);

                // Create a WriteValue object with the new value
                var writeValue = new WriteValue
                {
                    NodeId = nodeId,
                    AttributeId = Attributes.Value,
                    Value = new DataValue(new Variant(castedValue))
                };

                // Write the new value to the node
                //new RequestHeader() if needed
                Prg.m_session.Write(null, new WriteValueCollection { writeValue }, out StatusCodeCollection statusCodeCollection, out DiagnosticInfoCollection diagnosticInfo);

                // Check the results to make sure the write succeeded
                if (statusCodeCollection[0].Code != Opc.Ua.StatusCodes.Good)
                {
                    Utils.Trace("Error: failed to write data");
                }
                else
                {
                    Utils.Trace($"Wrote value {castedValue} to node {nodeId}");
                }
            }
            catch (Exception ex)
            {
                Utils.Trace("Error: " + ex.ToString());              
            }
        }
    }
}

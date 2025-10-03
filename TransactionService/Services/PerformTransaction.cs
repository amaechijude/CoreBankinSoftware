// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Grpc.Core;
// using Grpc.Net.Client;
// using SharedGrpcContracts.Protos.Account.V1;

// namespace TransactionService.Services
// {
//     public class PerformTransaction
//     {
//         public async Task<object> GetAccountDetails(string accountNumber)
//         {
//             try
//             {
//                 var gRpcChannel = GrpcChannel.ForAddress("https://localhost:7248/");
//                 var client = new AccountGrpcApiService.AccountGrpcApiServiceClient(gRpcChannel);

//                 var response = await client.GetAccountByIdAsync(new GetAccountByCustomerIdRequest
//                 {
//                     AccountId = accountNumber
//                 });
//                 return response;
//             }
//             catch (RpcException ex)
//             {
//                 // Handle gRPC-specific exceptions
//                 return new { Error = ex.Status.Detail };
//             }
//             catch (Exception ex)
//             {
//                 // Log or handle the exception as needed
//                 return new { Error = ex.Message };
//             }
//         }
//     }
// }
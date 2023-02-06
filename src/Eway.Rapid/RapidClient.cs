﻿using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Eway.Rapid.Abstractions.Interfaces;
using Eway.Rapid.Abstractions.Request;
using Eway.Rapid.Abstractions.Response;

namespace Eway.Rapid
{
    public class RapidClient : IRapidClient
    {
        private readonly JsonSerializerOptions _requestJsonSerializerOptions;
        private readonly JsonSerializerOptions _responseJsonSerializerOptions;

        public RapidClient(HttpClient httpClient)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _requestJsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            _responseJsonSerializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
        }

        public HttpClient HttpClient { get; }

        public Task<DirectPaymentResponse> CreateTransaction(DirectPaymentRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<DirectPaymentResponse>("/Transaction", HttpMethod.Post, request, cancellationToken);
        }

        public Task<CreateTransparentRedirectResponse> CreateTransaction(CreateTransparentRedirectRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<CreateTransparentRedirectResponse>("/AccessCodes", HttpMethod.Post, request, cancellationToken);
        }

        public Task<CreateResponsiveSharedResponse> CreateTransaction(CreateResponsiveSharedRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<CreateResponsiveSharedResponse>("/AccessCodesShared", HttpMethod.Post, request, cancellationToken);
        }

        public Task<CancelAuthorisationResponse> CancelAuthorisation(CancelAuthorisationRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<CancelAuthorisationResponse>("/CancelAuthorisation", HttpMethod.Post, request, cancellationToken);
        }

        public Task<CaptureAuthorisationResponse> CaptureAuthorisation(CaptureAuthorisationRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<CaptureAuthorisationResponse>("/CapturePayment", HttpMethod.Post, request, cancellationToken);
        }

        public Task<RefundResponse> Refund(string transactionId, DirectRefundRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<RefundResponse>($"/Transaction/{transactionId}/Refund", HttpMethod.Post, request, cancellationToken);
        }

        public Task<EnrolDirectThreeDSecureResponse> EnrolDirectThreeDSecure(EnrolDirectThreeDSecureRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<EnrolDirectThreeDSecureResponse>("/3dsenrol", HttpMethod.Post, request, cancellationToken);
        }

        public Task<VerifyDirectThreeDSecureResponse> VerifyDirectThreeDSecure(VerifyDirectThreeDSecureRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<VerifyDirectThreeDSecureResponse>("/3dsverify", HttpMethod.Post, request, cancellationToken);
        }

        public Task<QueryAccessCodeResponse> QueryAccessCode(string accessCode, CancellationToken cancellationToken = default)
        {
            return SendRequest<QueryAccessCodeResponse>($"/AccessCode/{accessCode}", HttpMethod.Get, null, cancellationToken);
        }

        public Task<QueryTransactionResponse> QueryTransactionByTransactionId(string transactionId, CancellationToken cancellationToken = default)
        {
            return SendRequest<QueryTransactionResponse>($"/Transaction/{transactionId}", HttpMethod.Get, null, cancellationToken);
        }

        public Task<QueryTransactionResponse> QueryTransactionByInvoiceNumber(string invoiceNumber, CancellationToken cancellationToken = default)
        {
            return SendRequest<QueryTransactionResponse>($"/Transaction/InvoiceNumber/{invoiceNumber}", HttpMethod.Get, null, cancellationToken);
        }

        public Task<QueryTransactionResponse> QueryTransactionByInvoiceRef(string invoiceRef, CancellationToken cancellationToken = default)
        {
            return SendRequest<QueryTransactionResponse>($"/Transaction/InvoiceRef/{invoiceRef}", HttpMethod.Get, null, cancellationToken);
        }

        public Task<CustomerResponse> CreateCustomer(CustomerRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<CustomerResponse>("/Customer", HttpMethod.Post, request, cancellationToken);
        }

        public Task<CustomerResponse> UpdateCustomer(CustomerRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<CustomerResponse>("/Customer", HttpMethod.Put, request, cancellationToken);
        }

        public Task<QueryCustomerResponse> QueryCustomer(string tokenCustomerId, CancellationToken cancellationToken = default)
        {
            return SendRequest<QueryCustomerResponse>($"/Customer/{tokenCustomerId}", HttpMethod.Get, null, cancellationToken);
        }

        public Task<CodeLookupResponse> APICodeLookup(CodeLookupRequest request, CancellationToken cancellationToken = default)
        {
            return SendRequest<CodeLookupResponse>("/APICodeLookup", HttpMethod.Post, request, cancellationToken);
        }

        private async Task<TResponse> SendRequest<TResponse>(string path, HttpMethod method, object request = default, CancellationToken cancellationToken = default)
        {
            var httpRequest = new HttpRequestMessage(method, path);

            if (method != HttpMethod.Get)
            {
                httpRequest.Content = JsonContent.Create(request, options: _requestJsonSerializerOptions);
            }

            var response = await HttpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResponse>(_responseJsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        }
    }
}

using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using TechTalk.SpecFlow;

namespace SpecFlowProject1.StepDefinitions
{
    [Binding]
    public sealed class FuelOrdersStepDefs
    {
        private RestClient? _client;
        private RestResponse? _response;
        private string? _orderId;
        private int _orderCount;
        private int _actualOrderCount;
        private readonly string _url = $"https://qacandidatetest.ensek.io/ENSEK/";
        [Given(@"User is authorised")]
        public void UserIsAuthorised()
        {
            Console.WriteLine("The User is already authorised");
        }

        [When(@"User '(.*)' the test data")]
        public void UserResetTheTestData(string path)
        {
            string requestUrl = $"{_url}{path}";
            _client = new RestClient(_url);
            var request = new RestRequest(requestUrl, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            _response = _client.Execute(request);
        }
        [Then(@"the data should be cleared and reset to default values")]
        public void ThenTheDataShouldBeClearedAndResetToDefaultValues()
        {
            Assert.AreEqual(HttpStatusCode.OK, _response.StatusCode, "Expected status code 200 OK");
        }
        [When(@"User purchase a quantity of fuel type '(.*)' with '(.*)' using '(.*)' endpoint")]
        public void UserPurchaseAQuantityOfFuelTypeWithUsing(string id, string quantity, string path)
        {
            var identifier = int.Parse(id);
            var quantityToPurchase = int.Parse(quantity);
            var requestUrl = $"{_url}{path}/{identifier}/{quantityToPurchase}";
            _client = new RestClient(requestUrl);
            var request = new RestRequest(requestUrl, Method.Put);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            _response = _client.Execute(request);
        }

        [Then(@"User should recieve a confirmation for purchase")]
        public void ThenUserShouldRecieveAConfirmationForPurchase()
        {
            //Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");
            var responseMessage = _response.Content;
            var regex = new Regex(@"order id is (\b[0-9a-fA-F\-]{36}\b)");
            var match = regex.Match(responseMessage);
            if (match.Success)
            {
                _orderId = match.Groups[1].Value;
                Console.WriteLine($"Expected Order ID: " + _orderId);
            }
        }
        [Then(@"Verify each order should be present in the response from the '(.*)' endpoint")]
        public void VerifyEachOrderShouldBePresentInTheResponseFromTheEndpoint(string path)
        {
            var requestUrl = $"{_url}{path}";
            _client = new RestClient(_url);
            var request = new RestRequest(requestUrl);
            _response = _client.Execute(request);
            var jsonArrayObj = JArray.Parse(_response.Content);
            var orderIds = new List<string>();

            foreach (var item in jsonArrayObj)
            {
                // Extract both "id" and "Id" fields, if they exist
                var id = item["id"]?.ToString();
                if (id != null)
                {
                    orderIds.Add(id);
                }
            }
            Assert.IsTrue(orderIds.Contains(_orderId));
        }
        [Given(@"User have a list of all orders from the '(.*)' endpoint")]
        public void GivenIHaveAListOfAllOrdersFromTheEndpoint(string path)
        {
            var requestUrl = $"{_url}{path}";
            _client = new RestClient(_url);
            var request = new RestRequest(requestUrl);
            _response = _client.Execute(request);
            var jsonArrayObj = JArray.Parse(_response.Content);
            var orderIds = new List<string>();

            foreach (var item in jsonArrayObj)
            {
                var id = item["id"]?.ToString();
                if (id != null)
                {
                    orderIds.Add(id);
                }
            }
            _orderCount = orderIds.Count;
        }
        [When(@"User filter orders that have a purchase timestamp before the current date")]
        public void WhenIFilterOrdersThatHaveAPurchaseTimestampBeforeTheCurrentDate()
        {
            var currentTime = DateTime.UtcNow;
            var formattedTime = currentTime.ToString("ddd, d MMM yyyy HH:mm:ss 'GMT'", CultureInfo.CurrentCulture);
            var jsonArrayObj = JArray.Parse(_response.Content);
            var timestamps = new List<string>();

            foreach (var item in jsonArrayObj)
            {
                var id = item["time"]?.ToString();
                if (id != null)
                {
                    timestamps.Add(id);
                }
            }

            var orders = new List<string>();
            for (var i = 0; i < timestamps.Count; i++)
            {
                var timestamp = DateTime.Parse(timestamps[i], CultureInfo.CurrentCulture);
                var formattedDateTime = DateTime.Parse(formattedTime, CultureInfo.CurrentCulture);

                if (timestamp < formattedDateTime)
                {
                    orders.Add(timestamps[i]);
                }
            }

            _actualOrderCount = orders.Count;
        }
        [Then(@"the count should match the expected number of past orders")]
        public void ThenUserCountShouldMatchTheExpectedNumberOfPastOrders()
        {
            Assert.IsTrue(_actualOrderCount.Equals(_orderCount));
        }
        [When(@"User send a request with an invalid (.*) with (.*) using '(.*)' endpoint")]
        public void WhenISendARequestWithAnInvalidWithUsingEndpoint(int id, int quantity, string path)
        {
            var requestUrl = $"{_url}{path}/{id}/{quantity}";
            _client = new RestClient(requestUrl);
            var request = new RestRequest(requestUrl, Method.Put);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            _response = _client.Execute(request);
        }
        [Then(@"the status code should be (.*)")]
        public void ThenTheStatusCodeShouldBe(int expectedStatusCode)
        {
            var statusCode = (int)_response.StatusCode;
            Assert.IsTrue(statusCode.Equals(expectedStatusCode));
        }
    }
}

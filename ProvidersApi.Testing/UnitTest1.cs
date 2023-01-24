using DB;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ProvidersApi.Controllers;

namespace ProvidersApi.Testing
{
    public class EnterpriseControllerTests
    {

        [Fact]
        public void IsValidApiKeyy_Returns_The_Correct_Value()
        {

            // Arrange
            var correct_apikey = "12345678";
            var NameOfClass = A.Fake<Enterprise>();
            var controller = new EnterpriseController(NameOfClass);

            // Act
            var actionResult = controller.IsApiKeyValid();

            // Assert
            Assert.Equal(true, actionResult);

        }
    }
}
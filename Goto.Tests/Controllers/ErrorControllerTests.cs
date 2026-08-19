using DigitalLinkToolkit.Conversion.Model;
using DigitalLinkToolkit.Exceptions;
using Goto.Controllers;
using Goto.Controllers.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Goto.Tests.Controllers;

public static class ErrorControllerTests
{
    public static ErrorController Controller { get; set; } = new();

    #region HandleError

    [TestClass]
    public sealed class HandleError
    {
        [TestMethod]
        public void ShouldHandleInvalidDigitalLinkException()
        {
            var feature = new ExceptionHandlerFeature()
            {
                Error = new InvalidDigitalLinkException([
                    new ValidationIssue() { 
                        Code = "01", 
                        Key = "01" ,
                        Message = "Invalid AI",
                        Value = "0123456"
                    }
                ])
            };

            var result = Controller.HandleError(feature);

            var objectResult = Assert.IsInstanceOfType<ObjectResult>(result);
            var errorResponse = Assert.IsInstanceOfType<ErrorResponse>(objectResult.Value);

            Assert.AreEqual("BadRequest", errorResponse.Type);
            Assert.AreEqual("The request specified an invalid DigitalLink", errorResponse.Title);
            Assert.AreEqual("The provided digital link is invalid", errorResponse.Detail);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
            CollectionAssert.AreEquivalent(new[] { new ErrorDetail { Code = "01", Message = "Invalid AI (Key: '01', Value: '0123456')" } }, errorResponse.Errors.ToList());
        }

        [TestMethod]
        public void ShouldHandleDbUpdateException()
        {
            var feature = new ExceptionHandlerFeature()
            {
                Error = new DbUpdateException("Unable to commit changes")
            };

            var result = Controller.HandleError(feature);

            var objectResult = Assert.IsInstanceOfType<ObjectResult>(result);
            var errorResponse = Assert.IsInstanceOfType<ErrorResponse>(objectResult.Value);

            Assert.AreEqual("Conflict", errorResponse.Type);
            Assert.AreEqual("There is a conflict while registering the DigitalLink", errorResponse.Title);
            Assert.AreEqual("Unable to commit changes", errorResponse.Detail);
            Assert.AreEqual((int)HttpStatusCode.Conflict, objectResult.StatusCode);
        }

        [TestMethod]
        public void ShouldHandleArgumentOutOfRangeException()
        {
            var feature = new ExceptionHandlerFeature()
            {
                Error = new ArgumentOutOfRangeException("DigitalLink", "The parameter was not expected")
            };

            var result = Controller.HandleError(feature);

            var objectResult = Assert.IsInstanceOfType<ObjectResult>(result);
            var errorResponse = Assert.IsInstanceOfType<ErrorResponse>(objectResult.Value);

            Assert.AreEqual("BadRequest", errorResponse.Type);
            Assert.AreEqual("The request specified an invalid argument", errorResponse.Title);
            Assert.AreEqual("The parameter was not expected (Parameter 'DigitalLink')", errorResponse.Detail);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, objectResult.StatusCode);
        }

        [TestMethod]
        public void ShouldHandleUnknownError()
        {
            var feature = new ExceptionHandlerFeature()
            {
                Error = new Exception("Test exception")
            };

            var result = Controller.HandleError(feature);

            var objectResult = Assert.IsInstanceOfType<ObjectResult>(result);
            var errorResponse = Assert.IsInstanceOfType<ErrorResponse>(objectResult.Value);

            Assert.AreEqual("InternalError", errorResponse.Type);
            Assert.AreEqual("Unable to process the request", errorResponse.Title);
            Assert.AreEqual("Test exception", errorResponse.Detail);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
        }

        [TestMethod]
        public void ShouldHandleNoError()
        {
            var feature = new ExceptionHandlerFeature();

            var result = Controller.HandleError(feature);

            var objectResult = Assert.IsInstanceOfType<ObjectResult>(result);
            var errorResponse = Assert.IsInstanceOfType<ErrorResponse>(objectResult.Value);

            Assert.AreEqual("InternalError", errorResponse.Type);
            Assert.AreEqual("Unable to process the request", errorResponse.Title);
            Assert.AreEqual("An unexpected error occured", errorResponse.Detail);
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);
        }
    }

    #endregion
}

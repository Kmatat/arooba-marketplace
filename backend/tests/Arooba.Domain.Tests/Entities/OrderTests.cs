using Arooba.Domain.Entities;
using Arooba.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Arooba.Domain.Tests.Entities;

public class OrderTests
{
    #region Accept

    [Fact]
    public void Accept_FromPending_ShouldTransitionToAccepted()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);

        // Act
        var result = order.Accept();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Accepted);
    }

    [Theory]
    [InlineData(OrderStatus.Accepted)]
    [InlineData(OrderStatus.ReadyToShip)]
    [InlineData(OrderStatus.InTransit)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Returned)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.RejectedShipping)]
    public void Accept_FromInvalidStatus_ShouldFail(OrderStatus invalidStatus)
    {
        // Arrange
        var order = CreateOrder(invalidStatus);

        // Act
        var result = order.Accept();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only pending orders");
        order.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region MarkReadyToShip

    [Fact]
    public void MarkReadyToShip_FromAccepted_ShouldTransitionToReadyToShip()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Accepted);

        // Act
        var result = order.MarkReadyToShip();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.ReadyToShip);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.ReadyToShip)]
    [InlineData(OrderStatus.InTransit)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void MarkReadyToShip_FromInvalidStatus_ShouldFail(OrderStatus invalidStatus)
    {
        // Arrange
        var order = CreateOrder(invalidStatus);

        // Act
        var result = order.MarkReadyToShip();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only accepted orders");
        order.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region MarkInTransit

    [Fact]
    public void MarkInTransit_FromReadyToShip_ShouldTransitionToInTransit()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.ReadyToShip);

        // Act
        var result = order.MarkInTransit();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.InTransit);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Accepted)]
    [InlineData(OrderStatus.InTransit)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void MarkInTransit_FromInvalidStatus_ShouldFail(OrderStatus invalidStatus)
    {
        // Arrange
        var order = CreateOrder(invalidStatus);

        // Act
        var result = order.MarkInTransit();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only orders ready to ship");
        order.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region MarkDelivered

    [Fact]
    public void MarkDelivered_FromInTransit_ShouldTransitionToDelivered()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.InTransit);

        // Act
        var result = order.MarkDelivered();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Accepted)]
    [InlineData(OrderStatus.ReadyToShip)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void MarkDelivered_FromInvalidStatus_ShouldFail(OrderStatus invalidStatus)
    {
        // Arrange
        var order = CreateOrder(invalidStatus);

        // Act
        var result = order.MarkDelivered();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only orders in transit");
        order.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region Cancel

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Accepted)]
    public void Cancel_FromValidStatus_ShouldTransitionToCancelled(OrderStatus validStatus)
    {
        // Arrange
        var order = CreateOrder(validStatus);

        // Act
        var result = order.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Theory]
    [InlineData(OrderStatus.ReadyToShip)]
    [InlineData(OrderStatus.InTransit)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Returned)]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.RejectedShipping)]
    public void Cancel_FromInvalidStatus_ShouldFail(OrderStatus invalidStatus)
    {
        // Arrange
        var order = CreateOrder(invalidStatus);

        // Act
        var result = order.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only pending or accepted orders");
        order.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region Return

    [Fact]
    public void Return_FromDelivered_ShouldTransitionToReturned()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Delivered);

        // Act
        var result = order.Return();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Returned);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Accepted)]
    [InlineData(OrderStatus.ReadyToShip)]
    [InlineData(OrderStatus.InTransit)]
    [InlineData(OrderStatus.Returned)]
    [InlineData(OrderStatus.Cancelled)]
    public void Return_FromInvalidStatus_ShouldFail(OrderStatus invalidStatus)
    {
        // Arrange
        var order = CreateOrder(invalidStatus);

        // Act
        var result = order.Return();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only delivered orders");
        order.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region RejectShipping

    [Fact]
    public void RejectShipping_FromReadyToShip_ShouldTransitionToRejectedShipping()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.ReadyToShip);

        // Act
        var result = order.RejectShipping();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.RejectedShipping);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Accepted)]
    [InlineData(OrderStatus.InTransit)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public void RejectShipping_FromInvalidStatus_ShouldFail(OrderStatus invalidStatus)
    {
        // Arrange
        var order = CreateOrder(invalidStatus);

        // Act
        var result = order.RejectShipping();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only orders ready to ship");
        order.Status.Should().Be(invalidStatus);
    }

    #endregion

    #region Full Lifecycle - Happy Path

    [Fact]
    public void FullLifecycle_PendingToDelivered_ShouldSucceed()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);

        // Act & Assert - Pending -> Accepted
        order.Accept().IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Accepted);

        // Act & Assert - Accepted -> ReadyToShip
        order.MarkReadyToShip().IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.ReadyToShip);

        // Act & Assert - ReadyToShip -> InTransit
        order.MarkInTransit().IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.InTransit);

        // Act & Assert - InTransit -> Delivered
        order.MarkDelivered().IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void FullLifecycle_PendingToDeliveredThenReturned_ShouldSucceed()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);

        // Act - Full delivery flow
        order.Accept();
        order.MarkReadyToShip();
        order.MarkInTransit();
        order.MarkDelivered();

        // Act & Assert - Delivered -> Returned
        order.Return().IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Returned);
    }

    [Fact]
    public void FullLifecycle_PendingToCancelled_ShouldSucceed()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);

        // Act
        var result = order.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void FullLifecycle_AcceptedToCancelled_ShouldSucceed()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);
        order.Accept();

        // Act
        var result = order.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void FullLifecycle_ReadyToShipThenRejected_ShouldSucceed()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);
        order.Accept();
        order.MarkReadyToShip();

        // Act
        var result = order.RejectShipping();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.RejectedShipping);
    }

    #endregion

    #region Default Values

    [Fact]
    public void NewOrder_ShouldHaveDefaultPendingStatus()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.Status.Should().Be(OrderStatus.Pending);
        order.Items.Should().NotBeNull().And.BeEmpty();
        order.OrderNumber.Should().BeEmpty();
    }

    #endregion

    #region Cannot Skip Steps

    [Fact]
    public void CannotSkipFromPendingToReadyToShip()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);

        // Act
        var result = order.MarkReadyToShip();

        // Assert
        result.IsFailure.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void CannotSkipFromPendingToInTransit()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);

        // Act
        var result = order.MarkInTransit();

        // Assert
        result.IsFailure.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public void CannotSkipFromPendingToDelivered()
    {
        // Arrange
        var order = CreateOrder(OrderStatus.Pending);

        // Act
        var result = order.MarkDelivered();

        // Assert
        result.IsFailure.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Pending);
    }

    #endregion

    #region Helpers

    private static Order CreateOrder(OrderStatus status)
    {
        return new Order
        {
            OrderNumber = "ORD-001",
            CustomerId = Guid.NewGuid(),
            Status = status,
            TotalAmount = 500m,
            ShippingFee = 30m,
            VatAmount = 70m,
            PaymentMethod = PaymentMethod.Card
        };
    }

    #endregion
}

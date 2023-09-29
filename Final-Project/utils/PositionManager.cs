    /// <summary>
    /// The `PositionManager` class is responsible for managing trading positions and orders within the trading algorithm (`Algo`).
    /// It handles the placement and cancellation of buy and sell orders asynchronously.
    /// </summary>
    public class PositionManager : Robot
    {
        private Algo Algo { get; set; }
        private double Volume { get; set; }

        private int _numberOfPendingPlaceOrderOperations;     // Number of pending order placement operations.
        private int _numberOfPendingCancelOrderOperations;    // Number of pending order cancellation operations.

        private readonly int amountOfOrders = 2;   // The number of orders to place.

        /// <summary>
        /// Initializes an instance of the `PositionManager` class with a reference to the parent `Algo` trading algorithm and a specified trading volume.
        /// </summary>
        /// <param name="algo">The parent `Algo` trading algorithm.</param>
        /// <param name="volume">The trading volume for the positions managed by this instance.</param>
        public PositionManager(Algo algo, double volume = 0.01)
        {
            Algo = algo;

            // Ensure that the specified trading volume is valid and greater than or equal to the minimum allowed volume.
            if (volume < 0 || volume < Symbol.VolumeInUnitsMin)
            {
                Volume = Symbol.VolumeInUnitsMin;
            }
            else
            {
                Volume = volume;
            }
        }

        /// <summary>
        /// Asynchronously sets buy positions by placing limit orders for a specified number of orders.
        /// </summary>
        public void SetBuyPositionAsync()
        {
            for (int i = 0; i < amountOfOrders; i++)
            {
                Interlocked.Increment(ref _numberOfPendingPlaceOrderOperations);

                // Place a limit buy order with specific parameters and callback function.
                PlaceLimitOrderAsync(TradeType.Buy,
                                       SymbolName,
                                       Volume,
                                       Symbol.Ask - (Symbol.PipSize * 5),
                                       "Trade_" + i,
                                       OnOrderPlaced);
            }
        }

        /// <summary>
        /// Asynchronously sets sell positions by placing limit orders for a specified number of orders.
        /// </summary>
        public void SetSellPositionAsync()
        {
            for (int i = 0; i < amountOfOrders; i++)
            {
                Interlocked.Increment(ref _numberOfPendingPlaceOrderOperations);

                // Place a limit sell order with specific parameters and callback function.
                PlaceLimitOrderAsync(TradeType.Buy,
                                       SymbolName,
                                       Volume,
                                       Symbol.Bid + (Symbol.PipSize * 5),
                                       "Trade_" + i,
                                       OnOrderPlaced);
            }
        }

        /// <summary>
        /// Callback method executed when an order is successfully placed.
        /// </summary>
        /// <param name="result">The result of the order placement operation.</param>
        private void OnOrderPlaced(TradeResult result)
        {
            if (Interlocked.Decrement(ref _numberOfPendingPlaceOrderOperations) == 0)
            {
                Algo.Print("All orders have been placed.");
            }
        }

        /// <summary>
        /// Asynchronously cancels all pending orders with labels containing "Trade_".
        /// </summary>
        private void CancelAllPendingOrdersAsync()
        {
            var pendingOrders = Algo.PendingOrders.Where(o => o.Label.Contains("Trade_")).ToArray();

            foreach (var order in pendingOrders)
            {
                Interlocked.Increment(ref _numberOfPendingCancelOrderOperations);

                // Cancel a pending order with a specific callback function.
                CancelPendingOrderAsync(order, OnOrderCancel);
            }
        }

        /// <summary>
        /// Callback method executed when an order is successfully canceled.
        /// </summary>
        /// <param name="result">The result of the order cancellation operation.</param>
        private void OnOrderCancel(TradeResult result)
        {
            if (Interlocked.Decrement(ref _numberOfPendingCancelOrderOperations) == 0)
            {
                Algo.Print("All orders have been canceled.");
            }
        }
    }

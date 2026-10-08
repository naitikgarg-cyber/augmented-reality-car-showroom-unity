namespace UnityEngine.XR.ARCore
{
    /// <summary>
    /// The state of an asynchronous operation.
    /// Refer to Google's ARCore developer guide for more information: https://developers.google.com/ar/reference/c/group/ar-future#ar_future_state_pending
    /// </summary>
    public enum ArFutureState
    {
        /// <summary>
        /// The operation is still pending.
        /// The result of the operation isn't available yet and any associated callback hasn't yet been dispatched or invoked.
        /// Do not use this to check if the operation can be cancelled as the state can change from another thread between the calls to ArFuture_getState and ArFuture_cancel.
        /// </summary>
        AR_FUTURE_STATE_PENDING = 0,

        /// <summary>
        /// The operation has been cancelled.
        /// Any associated callback will never be invoked.
        /// </summary>
        AR_FUTURE_STATE_CANCELLED = 1,

        /// <summary>
        /// The operation is complete and the result is available.
        /// If a callback was associated with this future, it will soon be invoked with the result on the main thread, if it hasn't been invoked already.
        /// </summary>
        AR_FUTURE_STATE_DONE = 2,
    }
}

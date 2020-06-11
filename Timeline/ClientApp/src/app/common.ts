// This error is thrown when ui goes wrong with bad logic.
// Such as am variable should not be null, but it does.
// This error should never occur. If it does, it indicates there is some logic bug in codes.
export class UiLogicError extends Error {}

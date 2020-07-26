import { OperatorFunction } from 'rxjs';
import { catchError } from 'rxjs/operators';

export function convertError<T, NewError>(
  oldErrorType: { new (...args: never[]): unknown },
  newErrorType: { new (): NewError }
): OperatorFunction<T, T> {
  return catchError((error) => {
    if (error instanceof oldErrorType) {
      throw new newErrorType();
    }
    throw error;
  });
}

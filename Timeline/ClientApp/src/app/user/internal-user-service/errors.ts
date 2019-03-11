export abstract class LoginError extends Error { }

export class BadNetworkError extends LoginError {
  constructor() {
    super('Network is bad.');
  }
}

export class AlreadyLoginError extends LoginError {
  constructor() {
    super('Internal logical error. There is already a token saved. Please call validateUserLoginState first.');
  }
}

export class BadCredentialsError extends LoginError {
  constructor() {
    super('Username or password is wrong.');
  }
}

export class UnknownError extends LoginError {
  constructor(public internalError?: any) {
    super('Sorry, unknown error occured!');
  }
}

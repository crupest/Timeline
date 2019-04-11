export class BadNetworkError extends Error {
  constructor() {
    super('Network is bad.');
  }
}

export class AlreadyLoginError extends Error {
  constructor() {
    super('Internal logical error. There is already a token saved. Please call validateUserLoginState first.');
  }
}

export class BadCredentialsError extends Error {
  constructor() {
    super('Username or password is wrong.');
  }
}

export class UnknownError extends Error {
  constructor(public internalError?: any) {
    super('Sorry, unknown error occured!');
  }
}

export class ServerInternalError extends Error {
  constructor(message?: string) {
    super('Wrong server response. ' + message);
  }
}

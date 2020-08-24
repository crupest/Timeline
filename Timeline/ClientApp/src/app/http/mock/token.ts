import { AxiosError } from "axios";

import {
  IHttpTokenClient,
  HttpCreateTokenRequest,
  HttpCreateTokenResponse,
  HttpVerifyTokenRequest,
  HttpVerifyTokenResponse,
} from "../token";

import { mockPrepare } from "./common";
import { getUser, MockUserNotExistError, checkToken } from "./user";

export class MockHttpTokenClient implements IHttpTokenClient {
  // TODO: Mock bad credentials error.
  async create(req: HttpCreateTokenRequest): Promise<HttpCreateTokenResponse> {
    await mockPrepare("token.create");
    try {
      const user = await getUser(req.username);
      return {
        user,
        token: `token-${req.username}`,
      };
    } catch (e) {
      if (e instanceof MockUserNotExistError) {
        throw {
          isAxiosError: true,
          response: {
            status: 400,
          },
        } as Partial<AxiosError>;
      }
      throw e;
    }
  }

  async verify(req: HttpVerifyTokenRequest): Promise<HttpVerifyTokenResponse> {
    await mockPrepare("token.verify");
    try {
      const user = await getUser(checkToken(req.token));
      return {
        user,
      };
    } catch (e) {
      throw {
        isAxiosError: true,
        response: {
          status: 400,
        },
      } as Partial<AxiosError>;
    }
  }
}

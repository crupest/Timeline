import { random, without, range } from 'lodash';

import { BlobWithEtag, NotModified } from '../common';
import {
  IHttpTimelineClient,
  HttpTimelineInfo,
  TimelineVisibility,
  HttpTimelineListQuery,
  HttpTimelineNotExistError,
  HttpTimelinePostRequest,
  HttpTimelineNameConflictError,
  HttpTimelinePatchRequest,
  HttpTimelinePostInfo,
  HttpTimelinePostContent,
  HttpTimelinePostPostRequest,
  HttpTimelinePostNotExistError,
  HttpTimelineGenericPostInfo,
} from '../timeline';
import { HttpUser } from '../user';

import { mockStorage, sha1, mockPrepare } from './common';
import { getUser, MockUserNotExistError, checkToken } from './user';

async function getTimelineNameList(): Promise<string[]> {
  return (await mockStorage.getItem<string[]>('timelines')) ?? [];
}

async function setTimelineNameList(newOne: string[]): Promise<void> {
  await mockStorage.setItem<string[]>('timelines', newOne);
}

type TimelinePropertyKey =
  | 'uniqueId'
  | 'owner'
  | 'description'
  | 'visibility'
  | 'members'
  | 'currentPostId';

function getTimelinePropertyKey(
  name: string,
  property: TimelinePropertyKey
): string {
  return `timeline.${name}.${property}`;
}

function getTimelinePropertyValue<T>(
  name: string,
  property: TimelinePropertyKey
): Promise<T> {
  return mockStorage.getItem<T>(getTimelinePropertyKey(name, property));
}

function setTimelinePropertyValue<T>(
  name: string,
  property: TimelinePropertyKey,
  value: T
): Promise<void> {
  return mockStorage
    .setItem<T>(getTimelinePropertyKey(name, property), value)
    .then();
}

interface HttpTimelineInfoEx extends HttpTimelineInfo {
  memberUsernames: string[];
}

function createUniqueId(): string {
  const s = 'abcdefghijklmnopqrstuvwxz0123456789';
  let result = '';
  for (let i = 0; i < 16; i++) {
    result += s[random(0, s.length - 1)];
  }
  return result;
}

class MockTimelineNotExistError extends Error {
  constructor() {
    super('Timeline not exist.');
  }
}

class MockTimelineAlreadyExistError extends Error {
  constructor() {
    super('Timeline already exist.');
  }
}

async function getTimelineInfo(name: string): Promise<HttpTimelineInfoEx> {
  let owner: HttpUser;
  if (name.startsWith('@')) {
    const ownerUsername = name.substr(1);
    owner = await getUser(ownerUsername);
    const optionalUniqueId = await getTimelinePropertyValue<string | null>(
      name,
      'uniqueId'
    );
    if (optionalUniqueId == null) {
      await setTimelineNameList([...(await getTimelineNameList()), name]);
      await setTimelinePropertyValue(name, 'uniqueId', createUniqueId());
    }
  } else {
    const optionalOwnerUsername = await getTimelinePropertyValue<string | null>(
      name,
      'owner'
    );
    if (optionalOwnerUsername == null) {
      throw new MockTimelineNotExistError();
    } else {
      owner = await getUser(optionalOwnerUsername);
    }
  }

  const memberUsernames =
    (await getTimelinePropertyValue<string[] | null>(name, 'members')) ?? [];
  const members = await Promise.all(
    memberUsernames.map(async (username) => {
      return await getUser(username);
    })
  );

  return {
    name,
    uniqueId: await getTimelinePropertyValue<string>(name, 'uniqueId'),
    owner,
    description:
      (await getTimelinePropertyValue<string | null>(name, 'description')) ??
      '',
    visibility:
      (await getTimelinePropertyValue<TimelineVisibility | null>(
        name,
        'visibility'
      )) ?? 'Register',
    members,
    memberUsernames,
  };
}

async function createTimeline(name: string, owner: string): Promise<void> {
  const optionalOwnerUsername = await getTimelinePropertyValue<string | null>(
    name,
    'owner'
  );
  if (optionalOwnerUsername != null) {
    throw new MockTimelineAlreadyExistError();
  }

  await setTimelineNameList([...(await getTimelineNameList()), name]);
  await setTimelinePropertyValue(name, 'uniqueId', createUniqueId());
  await setTimelinePropertyValue(name, 'owner', owner);
}

type TimelinePostPropertyKey =
  | 'type'
  | 'data'
  | 'etag'
  | 'author'
  | 'time'
  | 'lastUpdated';

function getTimelinePostPropertyKey(
  timelineName: string,
  id: number,
  propertyKey: TimelinePostPropertyKey
): string {
  return `timeline.${timelineName}.posts.${id}.${propertyKey}`;
}

function getTimelinePostPropertyValue<T>(
  timelineName: string,
  id: number,
  propertyKey: TimelinePostPropertyKey
): Promise<T> {
  return mockStorage.getItem<T>(
    getTimelinePostPropertyKey(timelineName, id, propertyKey)
  );
}

function setTimelinePostPropertyValue<T>(
  timelineName: string,
  id: number,
  propertyKey: TimelinePostPropertyKey,
  value: T
): Promise<T> {
  return mockStorage.setItem(
    getTimelinePostPropertyKey(timelineName, id, propertyKey),
    value
  );
}

function removeTimelinePostProperty(
  timelineName: string,
  id: number,
  propertyKey: TimelinePostPropertyKey
): Promise<void> {
  return mockStorage.removeItem(
    getTimelinePostPropertyKey(timelineName, id, propertyKey)
  );
}

async function getTimelinePostInfo(
  timelineName: string,
  id: number
): Promise<HttpTimelineGenericPostInfo> {
  const currentPostId = await getTimelinePropertyValue<number | null>(
    timelineName,
    'currentPostId'
  );
  if (currentPostId == null || id > currentPostId) {
    throw new HttpTimelinePostNotExistError();
  }

  const type = await getTimelinePostPropertyValue<string | null>(
    timelineName,
    id,
    'type'
  );

  if (type == null) {
    return {
      id,
      author: await getUser(
        await getTimelinePostPropertyValue<string>(timelineName, id, 'author')
      ),
      time: new Date(
        await getTimelinePostPropertyValue<string>(timelineName, id, 'time')
      ),
      lastUpdated: new Date(
        await getTimelinePostPropertyValue<string>(
          timelineName,
          id,
          'lastUpdated'
        )
      ),
      deleted: true,
    };
  } else {
    let content: HttpTimelinePostContent;
    if (type === 'text') {
      content = {
        type: 'text',
        text: await getTimelinePostPropertyValue(timelineName, id, 'data'),
      };
    } else {
      content = {
        type: 'image',
      };
    }

    return {
      id,
      author: await getUser(
        await getTimelinePostPropertyValue<string>(timelineName, id, 'author')
      ),
      time: new Date(
        await getTimelinePostPropertyValue<string>(timelineName, id, 'time')
      ),
      lastUpdated: new Date(
        await getTimelinePostPropertyValue<string>(
          timelineName,
          id,
          'lastUpdated'
        )
      ),
      content,
      deleted: false,
    };
  }
}

export class MockHttpTimelineClient implements IHttpTimelineClient {
  async listTimeline(
    query: HttpTimelineListQuery
  ): Promise<HttpTimelineInfo[]> {
    await mockPrepare();
    return (
      await Promise.all(
        (await getTimelineNameList()).map((name) => getTimelineInfo(name))
      )
    ).filter((timeline) => {
      if (
        query.visibility != null &&
        query.visibility !== timeline.visibility
      ) {
        return false;
      }
      if (query.relate != null) {
        if (query.relateType === 'own') {
          if (timeline.owner.username !== query.relate) {
            return false;
          }
        } else if (query.relateType === 'join') {
          if (!timeline.memberUsernames.includes(query.relate)) {
            return false;
          }
        } else if (
          timeline.owner.username !== query.relate &&
          !timeline.memberUsernames.includes(query.relate)
        ) {
          return false;
        }
      }
      return true;
    });
  }

  async getTimeline(timelineName: string): Promise<HttpTimelineInfo> {
    await mockPrepare();
    try {
      return await getTimelineInfo(timelineName);
    } catch (e) {
      if (
        e instanceof MockTimelineNotExistError ||
        e instanceof MockUserNotExistError
      ) {
        throw new HttpTimelineNotExistError();
      }
      throw e;
    }
  }

  async postTimeline(
    req: HttpTimelinePostRequest,
    token: string
  ): Promise<HttpTimelineInfo> {
    await mockPrepare();
    const user = checkToken(token);
    try {
      await createTimeline(req.name, user);
    } catch (e) {
      if (e instanceof MockTimelineAlreadyExistError) {
        throw new HttpTimelineNameConflictError();
      }
      throw e;
    }
    return await getTimelineInfo(req.name);
  }

  async patchTimeline(
    timelineName: string,
    req: HttpTimelinePatchRequest,
    _token: string
  ): Promise<HttpTimelineInfo> {
    await mockPrepare();
    if (req.description != null) {
      await setTimelinePropertyValue(
        timelineName,
        'description',
        req.description
      );
    }
    if (req.visibility != null) {
      await setTimelinePropertyValue(
        timelineName,
        'visibility',
        req.visibility
      );
    }
    return await getTimelineInfo(timelineName);
  }

  async deleteTimeline(timelineName: string, _token: string): Promise<void> {
    await mockPrepare();
    await setTimelineNameList(
      without(await getTimelineNameList(), timelineName)
    );
    await mockStorage.removeItem(
      getTimelinePropertyKey(timelineName, 'uniqueId')
    );

    // TODO: remove other things
  }

  async memberPut(
    timelineName: string,
    username: string,
    _token: string
  ): Promise<void> {
    await mockPrepare();
    const oldMembers =
      (await getTimelinePropertyValue<string[] | null>(
        timelineName,
        'members'
      )) ?? [];
    if (!oldMembers.includes(username)) {
      await setTimelinePropertyValue(timelineName, 'members', [
        ...oldMembers,
        username,
      ]);
    }
  }

  async memberDelete(
    timelineName: string,
    username: string,
    _token: string
  ): Promise<void> {
    await mockPrepare();
    const oldMembers =
      (await getTimelinePropertyValue<string[] | null>(
        timelineName,
        'members'
      )) ?? [];
    if (oldMembers.includes(username)) {
      await setTimelinePropertyValue(
        timelineName,
        'members',
        without(oldMembers, username)
      );
    }
  }

  listPost(
    timelineName: string,
    token?: string
  ): Promise<HttpTimelinePostInfo[]>;
  listPost(
    timelineName: string,
    token: string | undefined,
    query: {
      modifiedSince?: Date;
      includeDeleted?: false;
    }
  ): Promise<HttpTimelinePostInfo[]>;
  listPost(
    timelineName: string,
    token: string | undefined,
    query: {
      modifiedSince?: Date;
      includeDeleted: true;
    }
  ): Promise<HttpTimelineGenericPostInfo[]>;
  async listPost(
    timelineName: string,
    _token?: string,
    query?: {
      modifiedSince?: Date;
      includeDeleted?: boolean;
    }
  ): Promise<HttpTimelineGenericPostInfo[]> {
    await mockPrepare();
    // TODO: Permission check.

    const currentPostId = await getTimelinePropertyValue<number | null>(
      timelineName,
      'currentPostId'
    );

    return (
      await Promise.all(
        range(1, currentPostId == null ? 1 : currentPostId + 1).map(
          async (id) => {
            return await getTimelinePostInfo(timelineName, id);
          }
        )
      )
    )
      .filter((post) => {
        if (query?.includeDeleted !== true && post.deleted) {
          return false;
        }
        return true;
      })
      .filter((post) => {
        if (query?.modifiedSince != null) {
          return post.lastUpdated >= query.modifiedSince;
        }
        return true;
      });
  }

  getPostData(
    timelineName: string,
    postId: number,
    token: string
  ): Promise<BlobWithEtag>;
  async getPostData(
    timelineName: string,
    postId: number,
    _token?: string,
    etag?: string
  ): Promise<BlobWithEtag | NotModified> {
    await mockPrepare();
    // TODO: Permission check.

    const optionalSavedEtag = await getTimelinePostPropertyValue<string>(
      timelineName,
      postId,
      'etag'
    );

    if (optionalSavedEtag == null) {
      const optionalType = await getTimelinePostPropertyValue<string>(
        timelineName,
        postId,
        'type'
      );

      if (optionalType != null) {
        throw new Error('Post of this type has no data.');
      } else {
        throw new HttpTimelinePostNotExistError();
      }
    }

    if (etag === optionalSavedEtag) {
      return new NotModified();
    }

    return {
      data: await getTimelinePostPropertyValue<Blob>(
        timelineName,
        postId,
        'data'
      ),
      etag: optionalSavedEtag,
    };
  }

  async postPost(
    timelineName: string,
    req: HttpTimelinePostPostRequest,
    token: string
  ): Promise<HttpTimelinePostInfo> {
    await mockPrepare();
    const user = checkToken(token);

    const savedId = await getTimelinePropertyValue<number | null>(
      timelineName,
      'currentPostId'
    );
    const id = savedId ? savedId + 1 : 1;
    await setTimelinePropertyValue(timelineName, 'currentPostId', id);

    await setTimelinePostPropertyValue(timelineName, id, 'author', user);

    const currentTimeString = new Date().toISOString();
    await setTimelinePostPropertyValue(
      timelineName,
      id,
      'lastUpdated',
      currentTimeString
    );

    await setTimelinePostPropertyValue(
      timelineName,
      id,
      'time',
      req.time != null ? req.time.toISOString() : currentTimeString
    );

    const { content } = req;
    if (content.type === 'text') {
      await setTimelinePostPropertyValue(timelineName, id, 'type', 'text');
      await setTimelinePostPropertyValue(
        timelineName,
        id,
        'data',
        content.text
      );
    } else {
      await setTimelinePostPropertyValue(timelineName, id, 'type', 'image');
      await setTimelinePostPropertyValue(
        timelineName,
        id,
        'data',
        content.data
      );
      await setTimelinePostPropertyValue(
        timelineName,
        id,
        'etag',
        await sha1(content.data)
      );
    }

    return (await getTimelinePostInfo(
      timelineName,
      id
    )) as HttpTimelinePostInfo;
  }

  async deletePost(
    timelineName: string,
    postId: number,
    _token: string
  ): Promise<void> {
    await mockPrepare();
    // TODO: permission check
    await removeTimelinePostProperty(timelineName, postId, 'type');
    await removeTimelinePostProperty(timelineName, postId, 'data');
    await removeTimelinePostProperty(timelineName, postId, 'etag');
    await setTimelinePostPropertyValue(
      timelineName,
      postId,
      'lastUpdated',
      new Date().toISOString()
    );
  }
}

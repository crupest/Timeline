import { setHttpTokenClient } from "../token";
import { setHttpUserClient } from "../user";
import { setHttpTimelineClient } from "../timeline";

import { MockHttpTokenClient } from "./token";
import { MockHttpUserClient } from "./user";
import { MockHttpTimelineClient } from "./timeline";

setHttpTokenClient(new MockHttpTokenClient());
setHttpUserClient(new MockHttpUserClient());
setHttpTimelineClient(new MockHttpTimelineClient());

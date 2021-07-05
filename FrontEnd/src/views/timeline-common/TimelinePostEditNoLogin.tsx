import React from "react";
import { Trans } from "react-i18next";
import { Link } from "react-router-dom";

import TimelinePostEditCard from "./TimelinePostEditCard";

export default function TimelinePostEditNoLogin(): React.ReactElement | null {
  return (
    <TimelinePostEditCard>
      <div className="mt-3 mb-4">
        <Trans
          i18nKey="timeline.postNoLogin"
          components={{ l: <Link to="/login" /> }}
        />
      </div>
    </TimelinePostEditCard>
  );
}

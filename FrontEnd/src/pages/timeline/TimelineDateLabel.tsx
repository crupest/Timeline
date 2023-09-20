import TimelinePostContainer from "./TimelinePostContainer";

import "./TimelineDateLabel.css";

export default function TimelineDateLabel({ date }: { date: Date }) {
  return (
    <TimelinePostContainer>
      <div className="timeline-post-date-badge">
        {date.toLocaleDateString()}
      </div>
    </TimelinePostContainer>
  );
}

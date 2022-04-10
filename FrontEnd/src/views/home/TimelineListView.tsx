import React from "react";

import { convertI18nText, I18nText } from "@/common";

import { HttpTimelineInfo } from "@/http/timeline";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

interface TimelineListItemProps {
  timeline: HttpTimelineInfo;
}

const TimelineListItem: React.FC<TimelineListItemProps> = ({ timeline }) => {
  return (
    <div className="home-timeline-list-item home-timeline-list-item-timeline">
      <svg className="home-timeline-list-item-line" viewBox="0 0 120 100">
        <path
          d="M 80,50 m 0,-12 a 12 12 180 1 1 0,24 12 12 180 1 1 0,-24 z M 60,0 h 40 v 100 h -40 z"
          fillRule="evenodd"
          fill="#007bff"
        />
      </svg>
      <div>
        <div>{timeline.title}</div>
        <div>
          <small className="text-secondary">{timeline.description}</small>
        </div>
      </div>
      <Link to={`${timeline.owner.username}/${timeline.nameV2}`}>
        <i className="icon-button bi-arrow-right ms-3" />
      </Link>
    </div>
  );
};

const TimelineListArrow: React.FC = () => {
  return (
    <div>
      <div className="home-timeline-list-item">
        <svg className="home-timeline-list-item-line" viewBox="0 0 120 60">
          <path d="M 60,0 h 40 v 20 l -20,20 l -20,-20 z" fill="#007bff" />
        </svg>
      </div>
      <div className="home-timeline-list-item">
        <svg
          className="home-timeline-list-item-line home-timeline-list-loading-head"
          viewBox="0 0 120 40"
        >
          <path
            d="M 60,10 l 20,20 l 20,-20"
            fill="none"
            stroke="#007bff"
            strokeWidth="5"
          />
        </svg>
      </div>
    </div>
  );
};

interface TimelineListViewProps {
  headerText?: I18nText;
  timelines?: HttpTimelineInfo[];
}

const TimelineListView: React.FC<TimelineListViewProps> = ({
  headerText,
  timelines,
}) => {
  const { t } = useTranslation();

  return (
    <div className="home-timeline-list">
      <div className="home-timeline-list-item">
        <svg className="home-timeline-list-item-line" viewBox="0 0 120 120">
          <path
            d="M 0,20 Q 80,20 80,80 l 0,40"
            stroke="#007bff"
            strokeWidth="40"
            fill="none"
          />
        </svg>
        <h3>{convertI18nText(headerText, t)}</h3>
      </div>
      {timelines != null
        ? timelines.map((t) => <TimelineListItem key={t.nameV2} timeline={t} />)
        : null}
      <TimelineListArrow />
    </div>
  );
};

export default TimelineListView;

import { TimelineVisibility } from "@/http/timeline";
import XRegExp from "xregexp";

const timelineNameReg = XRegExp("^[-_\\p{L}]*$", "u");

export function validateTimelineName(name: string): boolean {
  return timelineNameReg.test(name);
}

export const timelineVisibilityTooltipTranslationMap: Record<
  TimelineVisibility,
  string
> = {
  Public: "timeline.visibilityTooltip.public",
  Register: "timeline.visibilityTooltip.register",
  Private: "timeline.visibilityTooltip.private",
};

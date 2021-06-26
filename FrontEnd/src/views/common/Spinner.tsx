import { PaletteColorType } from "@/palette";
import React from "react";

export interface SpinnerProps {
  size?: "sm" | "md" | "lg" | number;
  color?: PaletteColorType;
}

export default function Spinner(
  props: SpinnerProps
): React.ReactElement | null {
  return <span />;
}

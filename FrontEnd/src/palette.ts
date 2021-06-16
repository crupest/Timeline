import Color from "color";
import { BehaviorSubject, Observable } from "rxjs";

import refreshAnimation from "./utilities/refreshAnimation";

function lightenBy(color: Color, ratio: number): Color {
  const lightness = color.lightness();
  return color.lightness(lightness + (100 - lightness) * ratio);
}

function darkenBy(color: Color, ratio: number): Color {
  const lightness = color.lightness();
  return color.lightness(lightness - lightness * ratio);
}

export interface PaletteColor {
  color: string;
  lighter: string;
  darker: string;
  inactive: string;
  [key: string]: string;
}

const paletteColorList = [
  "primary",
  "primary-enhance",
  "secondary",
  "text-primary",
  "text-on-primary",
  "danger",
  "success",
] as const;

export type PaletteColorType = typeof paletteColorList[number];

export type Palette = Record<PaletteColorType, PaletteColor>;

export function generatePaletteColor(color: string): PaletteColor {
  const c = Color(color);
  return {
    color: c.rgb().toString(),
    inactive: (c.lightness() > 60 ? darkenBy(c, 0.1) : lightenBy(c, 0.1))
      .rgb()
      .toString(),
    lighter: lightenBy(c, 0.1).rgb().toString(),
    darker: darkenBy(c, 0.1).rgb().toString(),
  };
}

export function generatePalette(options: {
  primary: string;
  primaryEnhance?: string;
  secondary?: string;
}): Palette {
  const { primary, primaryEnhance, secondary } = options;
  const p = Color(primary);
  const pe =
    primaryEnhance == null
      ? lightenBy(p, 0.3).saturate(0.3)
      : Color(primaryEnhance);
  const s = secondary == null ? p.rotate(90) : Color(secondary);

  return {
    primary: generatePaletteColor(p.toString()),
    "primary-enhance": generatePaletteColor(pe.toString()),
    secondary: generatePaletteColor(s.toString()),
    "text-primary": generatePaletteColor("#111111"),
    "text-on-primary": generatePaletteColor(
      p.lightness() > 60 ? "black" : "white"
    ),
    danger: generatePaletteColor("red"),
    success: generatePaletteColor("green"),
  };
}

export function generatePaletteCSS(palette: Palette): string {
  const colors: [string, string][] = [];
  for (const colorType of paletteColorList) {
    const paletteColor = palette[colorType];
    for (const variant in paletteColor) {
      let key = `--tl-${colorType}`;
      if (variant !== "color") key += `-${variant}`;
      key += "-color";
      colors.push([key, paletteColor[variant]]);
    }
  }

  return `:root {${colors
    .map(([key, color]) => `${key} : ${color};`)
    .join("")}}`;
}

const paletteSubject: BehaviorSubject<Palette | null> =
  new BehaviorSubject<Palette | null>(null);

export const palette$: Observable<Palette | null> =
  paletteSubject.asObservable();

palette$.subscribe((palette) => {
  const styleTagId = "timeline-palette-css";
  if (palette != null) {
    let styleTag = document.getElementById(styleTagId);
    if (styleTag == null) {
      styleTag = document.createElement("style");
      styleTag.id = styleTagId;
      document.head.append(styleTag);
    }
    styleTag.innerHTML = generatePaletteCSS(palette);
  } else {
    const styleTag = document.getElementById(styleTagId);
    if (styleTag != null) {
      styleTag.parentElement?.removeChild(styleTag);
    }
  }

  refreshAnimation();
});

export function setPalette(palette: Palette): () => void {
  const old = paletteSubject.value;

  paletteSubject.next(palette);

  return () => {
    paletteSubject.next(old);
  };
}

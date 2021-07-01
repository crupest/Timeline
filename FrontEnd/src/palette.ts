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
  l1: string;
  l2: string;
  l3: string;
  d1: string;
  d2: string;
  d3: string;
  f1: string;
  f2: string;
  f3: string;
  r1: string;
  r2: string;
  r3: string;
  t: string;
  t1: string;
  t2: string;
  t3: string;
  [key: string]: string;
}

const paletteColorList = [
  "primary",
  "primary-enhance",
  "secondary",
  "danger",
  "success",
] as const;

export type PaletteColorType = typeof paletteColorList[number];

export type Palette = Record<PaletteColorType, PaletteColor>;

export function generatePaletteColor(color: string): PaletteColor {
  const c = Color(color);
  const light = c.lightness() > 60;
  const l1 = lightenBy(c, 0.1).rgb().toString();
  const l2 = lightenBy(c, 0.2).rgb().toString();
  const l3 = lightenBy(c, 0.3).rgb().toString();
  const d1 = darkenBy(c, 0.1).rgb().toString();
  const d2 = darkenBy(c, 0.2).rgb().toString();
  const d3 = darkenBy(c, 0.3).rgb().toString();
  const f1 = light ? l1 : d1;
  const f2 = light ? l2 : d2;
  const f3 = light ? l3 : d3;
  const r1 = light ? d1 : l1;
  const r2 = light ? d2 : l2;
  const r3 = light ? d3 : l3;
  const _t = light ? Color("black") : Color("white");
  const t = _t.rgb().toString();
  const _b = light ? lightenBy : darkenBy;
  const t1 = _b(_t, 0.1).rgb().toString();
  const t2 = _b(_t, 0.2).rgb().toString();
  const t3 = _b(_t, 0.3).rgb().toString();

  return {
    color: c.rgb().toString(),
    l1,
    l2,
    l3,
    d1,
    d2,
    d3,
    f1,
    f2,
    f3,
    r1,
    r2,
    r3,
    t,
    t1,
    t2,
    t3,
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
  const s = secondary == null ? Color("gray") : Color(secondary);

  return {
    primary: generatePaletteColor(p.toString()),
    "primary-enhance": generatePaletteColor(pe.toString()),
    secondary: generatePaletteColor(s.toString()),
    danger: generatePaletteColor("red"),
    success: generatePaletteColor("green"),
  };
}

export function generatePaletteCSS(palette: Palette): string {
  const colors: [string, string][] = [];
  for (const colorType of paletteColorList) {
    const paletteColor = palette[colorType];
    for (const variant in paletteColor) {
      let key = `--cru-${colorType}`;
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
  new BehaviorSubject<Palette | null>(
    // generatePalette({ primary: "rgb(0, 123, 255)" })
    null
  );

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

import Color from "color";
import { BehaviorSubject, Observable } from "rxjs";

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
  inactive: string;
  lighter: string;
  darker: string;
  [key: string]: string;
}

export interface Palette {
  primary: PaletteColor;
  primaryEnhance: PaletteColor;
  secondary: PaletteColor;
  textPrimary: PaletteColor;
  textOnPrimary: PaletteColor;
  danger: PaletteColor;
  success: PaletteColor;
  [key: string]: PaletteColor;
}

export function generatePaletteColor(color: string): PaletteColor {
  const c = Color(color);
  return {
    color: c.toString(),
    inactive: (c.lightness() > 60
      ? darkenBy(c, 0.1)
      : lightenBy(c, 0.2)
    ).toString(),
    lighter: lightenBy(c, 0.1).fade(0.1).toString(),
    darker: darkenBy(c, 0.1).toString(),
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
    primaryEnhance: generatePaletteColor(pe.toString()),
    secondary: generatePaletteColor(s.toString()),
    textPrimary: generatePaletteColor("#111111"),
    textOnPrimary: generatePaletteColor(p.lightness() > 60 ? "black" : "white"),
    danger: generatePaletteColor("red"),
    success: generatePaletteColor("green"),
  };
}

export function generatePaletteCSS(palette: Palette): string {
  function toSnakeCase(s: string): string {
    return s.replace(/[A-Z]/g, (letter) => `-${letter.toLowerCase()}`);
  }

  const colors: [string, string][] = [];
  for (const paletteColorName in palette) {
    const paletteColor = palette[paletteColorName];
    for (const variant in paletteColor) {
      let key = `--tl-${toSnakeCase(paletteColorName)}`;
      if (variant !== "color") key += `-${toSnakeCase(variant)}`;
      key += "-color";
      colors.push([key, paletteColor[variant]]);
    }
  }

  return `:root {${colors
    .map(([key, color]) => `${key} : ${color};`)
    .join("")}}`;
}

const paletteSubject: BehaviorSubject<Palette> = new BehaviorSubject<Palette>(
  generatePalette({ primary: "#007bff" })
);

export const palette$: Observable<Palette> = paletteSubject.asObservable();

palette$.subscribe((palette) => {
  let styleTag = document.getElementById("timeline-palette-css");
  if (styleTag == null) {
    styleTag = document.createElement("style");
    document.head.append(styleTag);
  }
  styleTag.innerHTML = generatePaletteCSS(palette);
});

export function setPalette(palette: Palette): () => void {
  const old = paletteSubject.value;

  paletteSubject.next(palette);

  return () => {
    paletteSubject.next(old);
  };
}

#!/usr/bin/env ts-node

/**
 * Color variable name scheme:
 * has variant: --[prefix]-[name]-[variant]-color: [color];
 * no variant: --[prefix]-[name]-color: [color];
 * Variant scheme:
 * [variant-prefix][level]
 * eg. --cru-primary-color: [color]; --cru-primary-l1-color: [color];
 */

class HslColor {
  constructor(
    public h: number,
    public s: number,
    public l: number,
  ) {}

  lighter(level: number): HslColor {
    return new HslColor(this.h, this.s, this.l + level * 10);
  }

  darker(level: number): HslColor {
    return new HslColor(this.h, this.s, this.l - level * 10);
  }

  toString(): string {
    return `hsl(${this.h} ${this.s}% ${this.l}%)`;
  }
}

class CssVarColor {
  constructor(
    public name: string,
    public cssVar: string,
  ) {}

  toString(): string {
    return `var(--${this.cssVar})`;
  }
}

class VariantColor {
  constructor(
    public color: HslColor,
    public variant?: string | null,
  ) {}

  toCssString(prefix: string, name: string): string {
    const variantPart = this.variant == null ? "" : `-${this.variant}`;
    return `--${prefix}-${name}${variantPart}-color: ${this.color.toString()};`;
  }
}

type LightnessVariantType = "lighter" | "darker";

interface LightnessVariant {
  prefix: string;
  type: LightnessVariantType;
}

function generateLightnessVariantColors(
  baseColor: HslColor,
  lightnessVariant: LightnessVariant,
  levels: number,
): VariantColor[] {
  const result: VariantColor[] = [];

  for (let i = 1; i <= levels; i++) {
    const color =
      lightnessVariant.type === "lighter"
        ? baseColor.lighter(i)
        : baseColor.darker(i);
    const colorVariant = `${lightnessVariant.prefix}${i}`;
    result.push(new VariantColor(color, colorVariant));
  }

  return result;
}

type ColorMode = "light" | "dark";

const themeVariantPrefixes = ["l", "d", "f", "b"] as const;

type LightnessVariants = {
  prefix: (typeof themeVariantPrefixes)[number];
  type: LightnessVariantType;
}[];

function generateThemeColorLightnessVariants(
  mode: ColorMode,
): LightnessVariants {
  return [
    {
      prefix: "l",
      type: "lighter",
    },
    {
      prefix: "d",
      type: "darker",
    },
    {
      prefix: "f",
      type: mode === "light" ? "lighter" : "darker",
    },
    {
      prefix: "b",
      type: mode === "light" ? "darker" : "lighter",
    },
  ];
}

class ColorGroup {
  constructor(
    public name: string,
    public baseColor: HslColor,
  ) {}

  generateVariantColors(
    lightnessVariants: LightnessVariant[],
    levels = 3,
  ): VariantColor[] {
    const result: VariantColor[] = [new VariantColor(this.baseColor)];

    for (const lightnessVariant of lightnessVariants) {
      result.push(
        ...generateLightnessVariantColors(
          this.baseColor,
          lightnessVariant,
          levels,
        ),
      );
    }

    return result;
  }
}

class ThemeColorGroup extends ColorGroup {
  constructor(name: string, baseColor: HslColor) {
    super(name, baseColor);
  }

  generateColors(mode: ColorMode): VariantColor[] {
    return super.generateVariantColors(
      generateThemeColorLightnessVariants(mode),
    );
  }

  generateCss(prefix: string, mode: ColorMode): string {
    return this.generateColors(mode)
      .map((c) => c.toCssString(prefix, this.name))
      .join("\n");
  }
}

class VarColorGroup {
  constructor(
    public name: string,
    public varName: string,
  ) {}

  generateCss(
    prefix: string,
    variantPrefixes = themeVariantPrefixes,
    levels = 3,
  ): string {
    const vs: string[] = [];
    for (const v of variantPrefixes) {
      for (let l = 1; l <= levels; l++) {
        vs.push(`${v}${l}`);
      }
    }
    let result = vs
      .map(
        (v) =>
          `--${prefix}-${this.name}-${v}-color: var(--${prefix}-${this.varName}-${v}-color);`,
      )
      .join("\n");
    result = `--${prefix}-${this.name}-color: var(--${prefix}-${this.varName}-color);\n${result}`;
    return result;
  }
}

const themeColorNames = [
  "primary",
  "secondary",
  "tertiary",
  "danger",
  "success",
] as const;

type ThemeColorNames = (typeof themeColorNames)[number];

type ThemeColors = {
  [key in ThemeColorNames]: HslColor;
};

// Config region begin
const prefix = "cru";

const themeColors: ThemeColors = {
  primary: new HslColor(210, 100, 50),
  secondary: new HslColor(40, 100, 50),
  tertiary: new HslColor(160, 100, 50),
  danger: new HslColor(0, 100, 50),
  success: new HslColor(120, 100, 50),
};

// Config region end

let output = "";

function indentText(
  text: string,
  level: number,
  indentWidth = 2,
  appendNewlines = 1,
): string {
  const lines = text.split("\n");
  const indent = " ".repeat(level * indentWidth);
  return (
    lines
      .map((line) => (line.length === 0 ? "" : `${indent}${line}`))
      .join("\n") + "\n".repeat(appendNewlines)
  );
}

function print(text: string, indent = 0, appendNewlines = 1) {
  output += indentText(text, indent, 2, appendNewlines);
}

function generateThemeColorCss(mode: ColorMode): string {
  let output = "";
  for (const name of themeColorNames) {
    const colorGroup = new ThemeColorGroup(name, themeColors[name]);
    output += colorGroup.generateCss(prefix, mode);
  }
  return output;
}

function generateThemeColorAliasCss(): string {
  let result = "";
  for (const name of themeColorNames) {
    const varColorGroup = new VarColorGroup("theme", name);
    result += `.${prefix}-${name} {\n${indentText(
      varColorGroup.generateCss(prefix),
      1,
    )}\n}\n`;
  }
  return result;
}

function main() {
  print("/* Generated by palette.ts */\n");

  print(":root {");
  print(generateThemeColorCss("light"), 1);
  print("}\n");

  print("@media (prefers-color-scheme: dark) {");
  print(":root {", 1);
  print(generateThemeColorCss("dark"), 2);
  print("}", 1);
  print("}\n");

  print(generateThemeColorAliasCss());
}

main();
process.stdout.write(output);

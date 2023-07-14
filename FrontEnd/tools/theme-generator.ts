#!/usr/bin/env ts-node

/**
 * Color variable name scheme:
 * has variant: --[prefix]-[name]-[variant]-color: [color];
 * no variant: --[prefix]-[name]-color: [color];
 * Variant scheme:
 * [variant-prefix][level]
 * eg. --cru-primary-color: [color]; --cru-primary-l1-color: [color];
 */

import { stdout } from "process";

interface CssSegment {
  toCssString(): string;
}

interface Color extends CssSegment {
  readonly type: "hsl" | "css-var";
  toString(): string;
}

class HslColor implements Color {
  readonly type = "hsl";

  constructor(
    public h: number,
    public s: number,
    public l: number,
  ) {}

  lighter(level: number): HslColor {
    return new HslColor(this.h, this.s, this.l + level * 5);
  }

  darker(level: number): HslColor {
    return new HslColor(this.h, this.s, this.l - level * 5);
  }

  toCssString(): string {
    return this.toString();
  }

  toString(): string {
    return `hsl(${this.h} ${this.s}% ${this.l}%)`;
  }

  static readonly white = new HslColor(0, 0, 100);
  static readonly black = new HslColor(0, 0, 0);
}

class ColorVariable implements CssSegment {
  constructor(
    public prefix: string,
    public name: string,
    public variant?: string | null,
  ) {}

  toString(): string {
    const variantPart = this.variant == null ? "" : `-${this.variant}`;
    return `--${this.prefix}-${this.name}${variantPart}-color`;
  }

  toCssString(): string {
    return this.toString();
  }
}

class CssVarColor implements Color {
  readonly type = "css-var";

  constructor(public colorVariable: ColorVariable) {}

  toCssString(): string {
    return this.toString();
  }

  toString(): string {
    return `var(${this.colorVariable.toString()})`;
  }
}

class ColorVariableDefinition implements CssSegment {
  constructor(
    public name: ColorVariable,
    public color: Color,
  ) {}

  toCssString(): string {
    return `${this.name.toCssString()}: ${this.color.toCssString()};`;
  }
}

type LightnessVariantType = "lighter" | "darker";

interface LightnessVariantInfo {
  prefix: string;
  type: LightnessVariantType;
  levels: number;
}

abstract class ColorGroup implements CssSegment {
  abstract getColorVariables(): ColorVariableDefinition[];
  toCssString(): string {
    return this.getColorVariables()
      .map((c) => c.toCssString())
      .join("\n");
  }
}

class LightnessVariantColorGroup extends ColorGroup {
  constructor(
    public prefix: string,
    public name: string,
    public baseColor: HslColor,
    public lightnessVariants: LightnessVariantInfo[],
  ) {
    super();
  }

  getColorVariables(): ColorVariableDefinition[] {
    const result: ColorVariableDefinition[] = [
      new ColorVariableDefinition(
        new ColorVariable(this.prefix, this.name),
        this.baseColor,
      ),
    ];

    for (const lightnessVariant of this.lightnessVariants) {
      for (let i = 1; i <= lightnessVariant.levels; i++) {
        const color =
          lightnessVariant.type === "lighter"
            ? this.baseColor.lighter(i)
            : this.baseColor.darker(i);
        const colorVariant = `${lightnessVariant.prefix}${i}`;
        result.push(
          new ColorVariableDefinition(
            new ColorVariable(this.prefix, this.name, colorVariant),
            color,
          ),
        );
      }
    }

    return result;
  }
}

class VarAliasColorGroup extends ColorGroup {
  constructor(
    public prefix: string,
    public newName: string,
    public oldName: string,
    public variants: string[],
  ) {
    super();
  }

  getColorVariables(): ColorVariableDefinition[] {
    const result = [
      new ColorVariableDefinition(
        new ColorVariable(this.prefix, this.newName),
        new CssVarColor(new ColorVariable(this.prefix, this.oldName)),
      ),
    ];
    for (const variant of this.variants) {
      result.push(
        new ColorVariableDefinition(
          new ColorVariable(this.prefix, this.newName, variant),
          new CssVarColor(
            new ColorVariable(this.prefix, this.oldName, variant),
          ),
        ),
      );
    }
    return result;
  }
}

class GrayscaleColorGroup extends ColorGroup {
  _delegate: LightnessVariantColorGroup;

  constructor(
    public prefix: string,
    public name: string,
    public baseColor: HslColor,
    public type: LightnessVariantType,
    public levels = 3,
  ) {
    super();

    this._delegate = new LightnessVariantColorGroup(
      prefix,
      name,
      this.baseColor,
      [{ prefix: "", type: this.type, levels }],
    );
  }

  getColorVariables(): ColorVariableDefinition[] {
    return this._delegate.getColorVariables();
  }

  static white(prefix: string, name: string, levels = 3): GrayscaleColorGroup {
    return new GrayscaleColorGroup(
      prefix,
      name,
      HslColor.white,
      "darker",
      levels,
    );
  }

  static black(prefix: string, name: string, levels = 3): GrayscaleColorGroup {
    return new GrayscaleColorGroup(
      prefix,
      name,
      HslColor.black,
      "lighter",
      levels,
    );
  }
}

class CompositeColorGroup extends ColorGroup {
  constructor(public groups: ColorGroup[]) {
    super();
  }

  getColorVariables(): ColorVariableDefinition[] {
    return this.groups
      .map((g) => g.getColorVariables())
      .reduce((prev, curr) => prev.concat(curr), []);
  }
}

type ThemeColors = { name: string; color: HslColor }[];

type ColorMode = "light" | "dark";

class Theme {
  static getDefaultThemeColorLightnessVariants(
    mode: ColorMode,
    levels = 3,
  ): LightnessVariantInfo[] {
    return [
      {
        prefix: "l",
        type: "lighter",
        levels,
      },
      {
        prefix: "d",
        type: "darker",
        levels,
      },
      {
        prefix: "f",
        type: mode === "light" ? "lighter" : "darker",
        levels,
      },
      {
        prefix: "b",
        type: mode === "light" ? "darker" : "lighter",
        levels,
      },
    ];
  }

  static getThemeColorAllVariants(): string[] {
    const lightnessVariantInfos =
      Theme.getDefaultThemeColorLightnessVariants("light");
    const result: string[] = [];
    for (const { prefix, levels } of lightnessVariantInfos) {
      for (let i = 1; i <= levels; i++) {
        result.push(`${prefix}${i}`);
      }
    }
    return result;
  }

  constructor(
    public prefix: string,
    public themeColors: ThemeColors,
    public levels = 3,
  ) {}

  getThemeColorDefinitions(mode: ColorMode): ColorGroup {
    const groups: ColorGroup[] = [];
    for (const { name, color } of this.themeColors) {
      const colorGroup = new LightnessVariantColorGroup(
        this.prefix,
        name,
        color,
        Theme.getDefaultThemeColorLightnessVariants(mode, this.levels),
      );
      groups.push(colorGroup);
    }
    return new CompositeColorGroup(groups);
  }

  getAliasColorDefinitions(name: string): ColorGroup {
    return new VarAliasColorGroup(
      this.prefix,
      "theme",
      name,
      Theme.getThemeColorAllVariants(),
    );
  }

  getGrayscaleDefinitions(mode: ColorMode): ColorGroup {
    const textGroup =
      mode === "light"
        ? GrayscaleColorGroup.black(this.prefix, "text", this.levels)
        : GrayscaleColorGroup.white(this.prefix, "text", this.levels);
    const bgGroup =
      mode === "light"
        ? GrayscaleColorGroup.white(this.prefix, "bg", this.levels)
        : GrayscaleColorGroup.black(this.prefix, "bg", this.levels);
    const lightGroup = GrayscaleColorGroup.white(
      this.prefix,
      "light",
      this.levels,
    );
    const darkGroup = GrayscaleColorGroup.black(
      this.prefix,
      "dark",
      this.levels,
    );
    const disabledGroup =
      mode == "light"
        ? new GrayscaleColorGroup(
            this.prefix,
            "disabled",
            new HslColor(0, 0, 75),
            "darker",
            this.levels,
          )
        : new GrayscaleColorGroup(
            this.prefix,
            "disabled",
            new HslColor(0, 0, 25),
            "lighter",
            this.levels,
          );
    return new CompositeColorGroup([
      textGroup,
      bgGroup,
      lightGroup,
      darkGroup,
      disabledGroup,
    ]);
  }

  generateCss(print: (text: string, indent: number) => void): void {
    print(":root {", 0);
    print(this.getThemeColorDefinitions("light").toCssString(), 1);
    print(this.getGrayscaleDefinitions("light").toCssString(), 1);
    print("}", 0);

    print("", 0);

    print("@media (prefers-color-scheme: dark) {", 0);
    print(":root {", 1);
    print(this.getThemeColorDefinitions("dark").toCssString(), 2);
    print(this.getGrayscaleDefinitions("dark").toCssString(), 2);
    print("}", 1);
    print("}", 0);

    print("", 0);

    for (const { name } of this.themeColors) {
      print(`.${this.prefix}-${name} {`, 0);
      print(this.getAliasColorDefinitions(name).toCssString(), 1);
      print("}", 0);

      print("", 0);
    }
  }
}

(function main() {
  const prefix = "cru";
  const themeColors: ThemeColors = [
    { name: "primary", color: new HslColor(210, 100, 50) },
    { name: "secondary", color: new HslColor(40, 100, 50) },
    { name: "tertiary", color: new HslColor(160, 100, 50) },
    { name: "danger", color: new HslColor(0, 100, 50) },
    { name: "success", color: new HslColor(120, 60, 50) },
  ];

  const theme = new Theme(prefix, themeColors);

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

  print("/* Generated by theme-generator.ts */\n");
  theme.generateCss(print);

  stdout.write(output);
})();

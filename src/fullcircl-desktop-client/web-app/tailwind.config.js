let colors_ = {
  gray: {
    900: "#202121", //darkest
    800: "#292828", //dark
    600: "#32302F", //DEFAULT
    500: "#3C3836", //light
    400: "#403C3A", //lighter
    300: "#45403D",
    200: "#504945",
    100: "#5A524C",
    50: "#665C54",  //lightest
  },
  white: {
    50: "#DDC7A1",  //DEFAULT
    100: "#D4BE98", 
    300: "#C5B18D",
    500: "#A89984", //dim
    700: "#928374", //dimmer
    900: "#7C6F64", //dimmest
  },
  aqua: {
    500: "#81BB83", //DEFAULT
    300: "#678E68", //darker
  },
  blue: {
    500: "#81BBB1", //DEFAULT
    300: "#649089", //darker
  },
  green: {
    500: "#A4C167", //DEFAULT
    300: "#879D58", //darker
  },
  orange: {
    500: "#E08552", //DEFAULT
    300: "#B66E45", //darker
  },
  red: {
    500: "#DB7070", //DEFAULT
    300: "#A85757", //darker
  },
  yellow: {
    500: "#E4AB67", //DEFAULT
    300: "#BF8A4A", //darker
  },
}

module.exports = {
  purge: {
    enabled: !process.env.ROLLUP_WATCH,
    mode: 'all',
    content: ['./**/**/*.html', './**/**/*.svelte'],

    options: {
      whitelistPatterns: [/svelte-/],
      defaultExtractor: (content) =>
        [...content.matchAll(/(?:class:)*([\w\d-/:%.]+)/gm)].map(([_match, group, ..._rest]) => group),
    },
  },
  darkMode: 'class',
  theme: {
    fontFamily: {
      'mono': ['"Ubuntu Mono"', 'ui-monospace', 'SFMono-Regular', 'Menlo', 'Monaco', 'Consolas', '"Liberation Mono"', '"Courier New"', 'monospace']
    },
    colors: {
      gray: {
        lightest: colors_.gray[50],
        lighter: colors_.gray[400],
        light: colors_.gray[500],
        DEFAULT: colors_.gray[600],
        dark: colors_.gray[800],
        darkest: colors_.gray[900],

        900: colors_.gray[900],
        800: colors_.gray[800],
        600: colors_.gray[600],
        500: colors_.gray[500],
        400: colors_.gray[400],
        300: colors_.gray[300],
        200: colors_.gray[200],
        100: colors_.gray[100],
        50: colors_.gray[50],
      },
      white: {
        DEFAULT: colors_.white[50],
        dimmer: colors_.white[700],
        dimmest: colors_.white[900],

        900: colors_.white[900],
        700: colors_.white[700],
        500: colors_.white[500],
        300: colors_.white[300],
        100: colors_.white[100],
        50: colors_.white[50],
      },
      aqua: {
        DEFAULT: colors_.aqua[500],
        dark: colors_.aqua[300],

        500: colors_.aqua[500],
        300: colors_.aqua[300],
      },
      blue: {
        DEFAULT: colors_.blue[500],
        dark: colors_.blue[300],

        500: colors_.blue[500],
        300: colors_.blue[300],
      },
      green: {
        DEFAULT: colors_.green[500],
        dark: colors_.green[300],

        500: colors_.green[500],
        300: colors_.green[300],
      },
      orange: {
        DEFAULT: colors_.orange[500],
        dark: colors_.orange[300],

        500: colors_.orange[500],
        300: colors_.orange[300],
      },
      red: {
        DEFAULT: colors_.red[500],
        dark: colors_.red[300],

        500: colors_.red[500],
        300: colors_.red[300],
      },
      yellow: {
        DEFAULT: colors_.yellow[500],
        dark: colors_.yellow[300],

        500: colors_.yellow[500],
        300: colors_.yellow[300],
      },
    },
    extend: {},
  },
  variants: {
    extend: {},
  },
  plugins: [],
}

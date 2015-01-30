﻿/*!
 * Prism Language JSON
 * @author CupOfTea696 https://github.com/CupOfTea696/prism/blob/gh-pages/components/prism-json.js
 */
Prism.languages.json = {
    'property': /"(\b|\B)[\w-]+"(?=\s*:)/ig,
    'number': /\b-?(0x[\dA-Fa-f]+|\d*\.?\d+([Ee]-?\d+)?)\b/g,
    'string': /"(?!:)(\\?[^'"])*?"(?!:)/g,
    'function': {
        pattern: /[a-z0-9_]+\(/ig,
        inside: {
            punctuation: /\(/
        }
    },
    'punctuation': /[{}[\]);,]/g,
    'operator': /:/g,
    'boolean': /\b(true|false)\b/gi,
    'null': /\bnull\b/gi,
};
Prism.languages.jsonp = Prism.languages.json;

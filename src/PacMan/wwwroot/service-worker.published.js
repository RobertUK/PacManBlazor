
self.assetsInclude = [/\.wav$/];


//const DEFAULT_ASSETS_INCLUDE = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.svg$/, /\.woff2$/, /\.ttf$/];
//const DEFAULT_ASSETS_EXCLUDE = [/^_content\/Bit.Bswup\/bit-bswup.sw.js$/, /^service-worker\.js$/];


self.assetsExclude = [/\.scp\.css$/, /weather\.json$/];
self.defaultUrl = 'index.html';
self.prohibitedUrls = [];
self.assetsUrl = '/service-worker-assets.js';

// more about SRI (Subresource Integrity) here: https://developer.mozilla.org/en-US/docs/Web/Security/Subresource_Integrity
// online tool to generate integrity hash: https://www.srihash.org/   or   https://laysent.github.io/sri-hash-generator/
// using only js to generate hash: https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/digest
self.externalAssets = [
    //{
    //    "hash": "sha256-lDAEEaul32OkTANWkZgjgs4sFCsMdLsR5NJxrjVcXdo=",
    //    "url": "css/app.css"
    //},
    {
        "url": "/"
    },
];

self.caseInsensitiveUrl = true;

self.serverHandledUrls = [/\/api\//];
self.serverRenderedUrls = [/\/privacy$/];

self.importScripts('_content/Bit.Bswup/bit-bswup.sw.js');

module.exports = [
"[turbopack-node]/transforms/postcss.ts { CONFIG => \"[project]/next-app/postcss.config.cjs [postcss] (ecmascript)\" } [postcss] (ecmascript, async loader)", ((__turbopack_context__) => {

__turbopack_context__.v((parentImport) => {
    return Promise.all([
  "build/chunks/36f10_286dd8a9._.js",
  "build/chunks/[root-of-the-server]__d4495e86._.js"
].map((chunk) => __turbopack_context__.l(chunk))).then(() => {
        return parentImport("[turbopack-node]/transforms/postcss.ts { CONFIG => \"[project]/next-app/postcss.config.cjs [postcss] (ecmascript)\" } [postcss] (ecmascript)");
    });
});
}),
];
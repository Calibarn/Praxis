import { readFile, readdir } from 'node:fs/promises';
import { dirname, posix, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const IMPORT_PATTERN =
  /(?:import|export)\s+(?:[^"']*?\s+from\s+)?["']([^"']+)["']|import\s*\(\s*["']([^"']+)["']\s*\)/g;

function normalize(value) {
  return value.replaceAll('\\', '/');
}

function projectOf(filePath) {
  const normalized = `/${normalize(filePath)}`;
  if (normalized.includes('/projects/shell/')) return 'shell';
  if (normalized.includes('/projects/news/')) return 'news';
  return undefined;
}

function targetOf(filePath, specifier) {
  if (specifier.startsWith('.')) {
    return normalize(posix.normalize(posix.join(posix.dirname(normalize(filePath)), specifier)));
  }
  return normalize(specifier);
}

export function findBoundaryViolations(filePath, source) {
  const importer = projectOf(filePath);
  if (!importer) return [];

  const violations = [];
  for (const match of source.matchAll(IMPORT_PATTERN)) {
    const specifier = match[1] ?? match[2];
    const target = targetOf(filePath, specifier);
    const targetProject = projectOf(target);

    if (targetProject && targetProject !== importer) {
      violations.push(`${importer} must not import ${targetProject} directly: ${specifier}`);
    }

    if (/(^|\/)(backend|gateway)(\/|$)/i.test(target)) {
      violations.push(`${importer} must not import backend or gateway internals: ${specifier}`);
    }
  }

  return violations;
}

async function sourceFiles(directory) {
  const entries = await readdir(directory, { withFileTypes: true });
  const files = [];
  for (const entry of entries) {
    const path = resolve(directory, entry.name);
    if (entry.isDirectory()) files.push(...(await sourceFiles(path)));
    else if (entry.isFile() && path.endsWith('.ts')) files.push(path);
  }
  return files;
}

async function main() {
  const workspace = resolve(dirname(fileURLToPath(import.meta.url)), '..');
  const projects = resolve(workspace, 'projects');
  const violations = [];

  for (const file of await sourceFiles(projects)) {
    const filePath = normalize(relative(workspace, file));
    const source = await readFile(file, 'utf8');
    for (const violation of findBoundaryViolations(filePath, source)) {
      violations.push(`${filePath}: ${violation}`);
    }
  }

  if (violations.length > 0) {
    console.error(violations.join('\n'));
    process.exitCode = 1;
    return;
  }

  console.log('Frontend architecture boundaries passed.');
}

const invokedPath = process.argv[1] ? resolve(process.argv[1]) : undefined;
if (invokedPath === fileURLToPath(import.meta.url)) await main();

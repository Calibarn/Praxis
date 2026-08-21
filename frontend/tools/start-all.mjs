import { spawn, spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

export function createProcessSpecs(host) {
  return ['shell', 'news'].map((name) => ({
    name,
    args: ['run', `start:${name}`, '--', '--host', host],
  }));
}

function readHost(args) {
  const hostIndex = args.indexOf('--host');
  return hostIndex === -1 ? '127.0.0.1' : args.at(hostIndex + 1);
}

export function terminateProcessTree(child, platform = process.platform) {
  if (!child.pid || child.exitCode !== null) return;

  if (platform === 'win32') {
    spawnSync('taskkill.exe', ['/pid', String(child.pid), '/T', '/F'], { stdio: 'ignore' });
    return;
  }

  try {
    process.kill(-child.pid, 'SIGTERM');
  } catch (error) {
    if (error.code !== 'ESRCH') throw error;
  }
}

export function startAll(
  host = '127.0.0.1',
  {
    spawnProcess = spawn,
    terminateTree = terminateProcessTree,
    processTarget = process,
    platform = process.platform,
  } = {},
) {
  if (!host) throw new Error('Für --host muss ein Wert angegeben werden.');

  const npmCommand = platform === 'win32' ? 'npm.cmd' : 'npm';
  const children = createProcessSpecs(host).map(({ name, args }) => {
    const child = spawnProcess(npmCommand, args, { detached: true, stdio: 'inherit' });
    child.on('error', (error) => {
      console.error(`${name} konnte nicht gestartet werden:`, error);
      stopAll(1);
    });
    return child;
  });

  let stopping = false;
  let requestedExitCode = 0;
  const exitedChildren = new Set();
  const stopAll = (exitCode = 0) => {
    if (stopping) return;
    stopping = true;
    requestedExitCode = exitCode;
    children.forEach((child) => terminateTree(child, platform));
  };

  for (const [signal, exitCode] of [
    ['SIGINT', 130],
    ['SIGTERM', 143],
  ]) {
    processTarget.on(signal, () => stopAll(exitCode));
  }

  children.forEach((child) => {
    child.on('close', (code) => {
      exitedChildren.add(child);
      if (!stopping) stopAll(code ?? 1);
      if (exitedChildren.size === children.length) {
        processTarget.exitCode = requestedExitCode;
      }
    });
  });

  return { children, stopAll };
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  startAll(readHost(process.argv.slice(2)));
}

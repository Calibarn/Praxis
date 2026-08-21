import assert from 'node:assert/strict';
import { EventEmitter } from 'node:events';
import test from 'node:test';

import { createProcessSpecs, startAll } from './start-all.mjs';

test('starts shell and news on their documented ports', () => {
  assert.deepEqual(createProcessSpecs('127.0.0.1'), [
    {
      name: 'shell',
      args: ['run', 'start:shell', '--', '--host', '127.0.0.1'],
    },
    {
      name: 'news',
      args: ['run', 'start:news', '--', '--host', '127.0.0.1'],
    },
  ]);
});

test('accepts the all-interface binding required inside Docker', () => {
  const specs = createProcessSpecs('0.0.0.0');

  assert.ok(specs.every(({ args }) => args.at(-1) === '0.0.0.0'));
});

test('stops both process trees when one child closes', () => {
  const children = [new EventEmitter(), new EventEmitter()];
  children.forEach((child, index) => {
    child.pid = index + 100;
    child.exitCode = null;
  });
  const terminated = [];
  const processTarget = new EventEmitter();
  processTarget.exitCode = undefined;

  startAll('127.0.0.1', {
    spawnProcess: () => children.shift(),
    terminateTree: (child) => terminated.push(child.pid),
    processTarget,
    platform: 'win32',
  }).children[0].emit('close', 1);

  assert.deepEqual(terminated, [100, 101]);
});

test('uses the conventional deterministic exit code for Ctrl+C', () => {
  const spawned = [new EventEmitter(), new EventEmitter()];
  spawned.forEach((child, index) => {
    child.pid = index + 200;
    child.exitCode = null;
  });
  const processTarget = new EventEmitter();
  const { children } = startAll('127.0.0.1', {
    spawnProcess: () => spawned.shift(),
    terminateTree: () => {},
    processTarget,
    platform: 'win32',
  });

  processTarget.emit('SIGINT');
  children.forEach((child) => child.emit('close', null));

  assert.equal(processTarget.exitCode, 130);
});

test('stops the sibling process tree when spawning a child fails', () => {
  const spawned = [new EventEmitter(), new EventEmitter()];
  spawned.forEach((child, index) => {
    child.pid = index + 300;
    child.exitCode = null;
  });
  const terminated = [];
  const processTarget = new EventEmitter();
  const { children } = startAll('127.0.0.1', {
    spawnProcess: () => spawned.shift(),
    terminateTree: (child) => terminated.push(child.pid),
    processTarget,
    platform: 'win32',
  });

  children[0].emit('error', new Error('spawn npm.cmd ENOENT'));
  children[0].emit('close', null);
  children[1].emit('close', null);

  assert.deepEqual(terminated, [300, 301]);
  assert.equal(processTarget.exitCode, 1);
});

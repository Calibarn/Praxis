import assert from 'node:assert/strict';
import { test } from 'node:test';

import { findBoundaryViolations } from './check-boundaries.mjs';

test('accepts imports that stay inside the shell boundary', () => {
  const violations = findBoundaryViolations(
    'projects/shell/src/app/app.ts',
    'import { Component } from "@angular/core";',
  );

  assert.deepEqual(violations, []);
});

test('rejects a direct shell import from the news remote', () => {
  const violations = findBoundaryViolations(
    'projects/news/src/app/illegal.ts',
    'import { App } from "../../../shell/src/app/app";',
  );

  assert.equal(violations.length, 1);
  assert.match(violations[0], /news.*shell/i);
});

test('rejects a direct news import from the shell', () => {
  const violations = findBoundaryViolations(
    'projects/shell/src/app/illegal.ts',
    'import { App } from "../../../news/src/app/app";',
  );

  assert.equal(violations.length, 1);
  assert.match(violations[0], /shell.*news/i);
});

test('rejects frontend imports that escape to backend internals', () => {
  const violations = findBoundaryViolations(
    'projects/news/src/app/illegal.ts',
    'import { repository } from "../../../../../backend/services/news-service/src/repository";',
  );

  assert.equal(violations.length, 1);
  assert.match(violations[0], /backend/i);
});

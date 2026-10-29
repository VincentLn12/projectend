import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative, sep } from 'node:path';

// Enforces the rule in /CLAUDE.md: only shared/ui may contain raw form controls, buttons and tables.
const APP_DIRECTORY = join(process.cwd(), 'src', 'app');
const SHARED_UI_DIRECTORY = join(APP_DIRECTORY, 'shared', 'ui');

const FORBIDDEN_ELEMENTS = ['input', 'select', 'textarea', 'button', 'table'];
const EXEMPT_MARKER = 'shared-ui-exempt:';

function collectTemplates(directory: string): string[] {
  const files: string[] = [];
  for (const entry of readdirSync(directory)) {
    const fullPath = join(directory, entry);
    if (statSync(fullPath).isDirectory()) {
      files.push(...collectTemplates(fullPath));
    } else if (entry.endsWith('.html')) {
      files.push(fullPath);
    }
  }
  return files;
}

function findViolations(filePath: string): string[] {
  const lines = readFileSync(filePath, 'utf8').split(/\r?\n/);
  const violations: string[] = [];

  lines.forEach((line, index) => {
    for (const element of FORBIDDEN_ELEMENTS) {
      const opensRawElement = new RegExp(`<${element}[\\s>/]`).test(line);
      if (!opensRawElement) {
        continue;
      }
      const isSharedButton = element === 'button' && /\bapp-button\b/.test(line);
      const previousLine = index > 0 ? lines[index - 1] : '';
      const isExempt = line.includes(EXEMPT_MARKER) || previousLine.includes(EXEMPT_MARKER);
      if (isSharedButton || isExempt) {
        continue;
      }
      violations.push(`${relative(APP_DIRECTORY, filePath)}:${index + 1} uses raw <${element}>`);
    }
  });

  return violations;
}

describe('shared components rule', () => {
  it('pages and layouts use shared components instead of raw controls', () => {
    const pageTemplates = collectTemplates(APP_DIRECTORY).filter(
      (file) => !file.startsWith(SHARED_UI_DIRECTORY + sep),
    );

    const violations = pageTemplates.flatMap(findViolations);

    expect(violations).toEqual([]);
  });
});

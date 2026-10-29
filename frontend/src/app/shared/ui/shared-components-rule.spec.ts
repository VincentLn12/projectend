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
  const content = readFileSync(filePath, 'utf8');
  const lines = content.split(/\r?\n/);
  const violations: string[] = [];
  // The whole opening tag, even when its attributes continue on following lines.
  const openingTag = new RegExp(`<(${FORBIDDEN_ELEMENTS.join('|')})(?=[\\s>/])[^>]*>`, 'g');

  for (const match of content.matchAll(openingTag)) {
    const element = match[1];
    const lineIndex = content.slice(0, match.index).split(/\r?\n/).length - 1;
    const isSharedButton = element === 'button' && /\bapp-button\b/.test(match[0]);
    const isExempt = lines[lineIndex].includes(EXEMPT_MARKER) || (lines[lineIndex - 1] ?? '').includes(EXEMPT_MARKER);
    if (!isSharedButton && !isExempt) {
      violations.push(`${relative(APP_DIRECTORY, filePath)}:${lineIndex + 1} uses raw <${element}>`);
    }
  }

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

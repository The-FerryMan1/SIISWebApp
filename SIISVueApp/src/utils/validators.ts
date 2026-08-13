export function isNonEmpty(value: string | undefined | null) {
  return !!(value && String(value).trim())
}

export function validateDateRange(start: string | undefined | null, end: string | undefined | null) {
  if (!start || !end) return { valid: false, message: 'Start date and end date are required' }
  const s = new Date(start)
  const e = new Date(end)
  if (Number.isNaN(s.getTime()) || Number.isNaN(e.getTime())) return { valid: false, message: 'Invalid date format' }
  if (s > e) return { valid: false, message: 'Start date must be before or equal to end date' }
  return { valid: true }
}

export function validateAccumulatedHours(value: number | undefined | null, max?: number) {
  if (value === null || value === undefined || Number.isNaN(Number(value))) return { valid: false, message: 'Accumulated hours must be a number' }
  if (value < 0) return { valid: false, message: 'Accumulated hours cannot be negative' }
  if (typeof max === 'number' && value > max) return { valid: false, message: `Accumulated hours cannot exceed ${max}` }
  return { valid: true }
}

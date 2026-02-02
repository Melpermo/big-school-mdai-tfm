# UI/UX Design Guidelines

## Visual Identity

### Core Aesthetic
- **Style:** Clean, minimalist, professional
- **Tone:** Sober, slightly ironic, non-judgmental
- **Target:** Mobile-first, readable at small sizes
- **Inspiration:** Reigns (mechanics), modern productivity apps (visual)

### Color Palette

**Primary Stats Colors:**
```
💰 Budget (Presupuesto)
├─ Primary: #2ECC71 (Green)
├─ Warning: #F39C12 (Orange)
└─ Critical: #E74C3C (Red)

⏳ Time (Tiempo)
├─ Primary: #3498DB (Blue)
├─ Warning: #F39C12 (Orange)
└─ Critical: #E74C3C (Red)

🧠 Morale (Moral)
├─ Primary: #9B59B6 (Purple)
├─ Warning: #F39C12 (Orange)
└─ Critical: #E74C3C (Red)

🧩 Quality (Calidad)
├─ Primary: #1ABC9C (Teal)
├─ Warning: #F39C12 (Orange)
└─ Critical: #E74C3C (Red)
```

**UI Elements:**
```
Background: #ECEFF1 (Light Gray)
Card Base: #FFFFFF (White)
Card Shadow: rgba(0,0,0,0.1)
Text Primary: #2C3E50 (Dark Blue-Gray)
Text Secondary: #7F8C8D (Medium Gray)
Accent: #34495E (Dark Gray-Blue)
```

**Card Category Colors (Subtle accents):**
```
⚙️ Technical: #546E7A (Blue-Gray)
👥 Team: #8E44AD (Purple)
💼 Business: #27AE60 (Green)
⏰ Time/Scope: #2980B9 (Blue)
🤖 AI: #E67E22 (Orange)
🌱 Opportunity: #16A085 (Teal)
```

### Typography

**Primary Font:** Sans-serif (Roboto, Inter, or system default)

```
Card Title: 20px, Bold, #2C3E50
Situation Text: 16px, Regular, #2C3E50, line-height: 1.5
Option Text: 18px, Medium, #2C3E50
Stat Values: 24px, Bold
Stat Labels: 14px, Regular, #7F8C8D
```

### Spacing System
```
xs: 4px   - Tight spacing, stat indicators
sm: 8px   - Card padding, button margins
md: 16px  - Standard spacing between elements
lg: 24px  - Section separators
xl: 32px  - Major section breaks
```

## Screen Layouts

### Game Screen (Main)

```
┌─────────────────────────────────┐
│  ┌─┐ ┌─┐ ┌─┐ ┌─┐               │  ← Stats HUD (always visible)
│  │💰│ │⏳│ │🧠│ │🧩│              │
│  └─┘ └─┘ └─┘ └─┘               │
│                                 │
│    ┌───────────────────┐       │
│    │                   │       │  ← Card area (center)
│    │   CARD CONTENT    │       │
│    │                   │       │
│    │  ← [Opción] →     │       │  ← Swipe indicators
│    │                   │       │
│    └───────────────────┘       │
│                                 │
│         Turn: 12/30             │  ← Turn counter (subtle)
│                                 │
└─────────────────────────────────┘
```

### Stats HUD Design

**Horizontal bar style (top of screen):**
```
┌──────────────────────────────────────┐
│  💰 65    ⏳ 42    🧠 78    🧩 50    │
│  ████▓▓░░  ████▒▒░░  ████████░  ████▓░░ │
└──────────────────────────────────────┘
```

**Features:**
- Icon + Numeric value
- Progress bar (0-100 scale)
- Color changes based on value:
  - 60-100: Primary color
  - 30-59: Warning color (orange)
  - 0-29: Critical color (red)
- Smooth animations on change

### Card Design

**Card Structure:**
```
┌─────────────────────────┐
│ ⚙️                      │ ← Category icon (subtle)
│                         │
│   Card Title            │ ← Bold, clear
│                         │
│   Situation text that   │ ← Readable, spaced
│   explains the current  │
│   dilemma clearly...    │
│                         │
│  ┌─────────────────┐   │ ← Image (optional)
│  │   [  Image  ]   │   │
│  └─────────────────┘   │
│                         │
│  ← Option A | Option B →│ ← Clear swipe cues
│                         │
└─────────────────────────┘
```

**Card Dimensions:**
- Aspect ratio: 3:4 (portrait)
- Mobile: ~80% screen width
- Corner radius: 12px
- Shadow: 0 4px 8px rgba(0,0,0,0.12)

**Card States:**
- **Neutral:** Centered, subtle shadow
- **Dragging Left:** Rotated -5°, left option highlighted
- **Dragging Right:** Rotated +5°, right option highlighted
- **Confirmed:** Quick animation out, new card fades in

### Swipe Interaction Design

**Visual Feedback:**
```
NEUTRAL STATE:
Card centered
Options visible but neutral
Subtle guide arrows: ← →

DRAG LEFT:
Card rotates left (-5° max)
Left option highlighted
Left side glows green/indicator
Background left tints slightly

DRAG RIGHT:
Card rotates right (+5° max)
Right option highlighted
Right side glows green/indicator
Background right tints slightly

THRESHOLD REACHED:
Haptic feedback (mobile)
Option fully highlighted
Slight scale increase (1.05x)

VALIDATED:
Card swooshes off screen
Quick stat update animation
New card slides in
```

**Thresholds:**
- **Decision Threshold:** Card must be dragged >100px horizontally
- **Return Threshold:** If released before threshold, card snaps back
- **Rotation:** Max 5° in direction of swipe
- **Resistance:** Optional slight resistance near center

### End Screen Design

**Defeat Screen:**
```
┌─────────────────────────────────┐
│                                 │
│         [Icon: 💥 or 📉]        │
│                                 │
│     PROJECT COLLAPSED           │
│                                 │
│   [Specific ending message]     │
│   "Your studio ran out of       │
│    budget after 18 turns..."    │
│                                 │
│  Final Stats:                   │
│  💰 Budget: 0                   │
│  ⏳ Time: 45                    │
│  🧠 Morale: 62                  │
│  🧩 Quality: 38                 │
│                                 │
│  [ Play Again ]  [ Main Menu ]  │
│                                 │
└─────────────────────────────────┘
```

**Victory Screen:**
```
┌─────────────────────────────────┐
│                                 │
│         [Icon: 🎉 or ⭐]        │
│                                 │
│     PROJECT SHIPPED!            │
│                                 │
│   [Quality-based message]       │
│   "Your game launched           │
│    successfully..."             │
│                                 │
│  Final Stats:                   │
│  💰 Budget: 34                  │
│  ⏳ Time: 23                    │
│  🧠 Morale: 71                  │
│  🧩 Quality: 68                 │
│                                 │
│  Turns Survived: 30             │
│                                 │
│  [ Play Again ]  [ Main Menu ]  │
│                                 │
└─────────────────────────────────┘
```

## Animation Guidelines

### Card Animations

**Card Entry:**
```
Duration: 0.3s
Easing: EaseOutQuad
Effect: Fade in + slight scale (0.95 → 1.0)
```

**Card Exit (decision made):**
```
Duration: 0.25s
Easing: EaseInQuad
Effect: Slide out in swipe direction + fade out
```

**Card Return (invalid swipe):**
```
Duration: 0.2s
Easing: EaseOutElastic
Effect: Snap back to center with slight bounce
```

### Stat Change Animations

**Value Update:**
```
Duration: 0.4s
Easing: EaseOutCubic
Effect: 
  - Number counts up/down
  - Progress bar animates smoothly
  - Color transition if crossing threshold
  - Optional: +/- indicator floats up
```

**Critical Warning:**
```
Duration: 0.5s
Easing: Pulse
Effect:
  - Stat bar pulses 2-3 times
  - Icon shakes slightly
  - Color shifts to red
```

### Screen Transitions

**Game Start:**
```
Duration: 0.5s
Effect: Fade in from black
```

**Game End:**
```
Duration: 0.4s
Effect: 
  - Card fades out
  - Screen dims
  - End screen slides up
```

## Mobile-Specific Considerations

### Touch Targets
- Minimum tap target: 44x44px
- Card drag area: entire card
- Buttons: 48px minimum height

### Gestures
- **Swipe:** Primary interaction
- **Tap:** (not used for decisions, prevents accidental choices)
- **Long Press:** (optional, could show card details)

### Haptic Feedback
- Light haptic on drag start
- Medium haptic on threshold reached
- Heavy haptic on decision confirmed
- Double tap haptic on critical stat warning

### Screen Orientations
- **Primary:** Portrait
- **Landscape:** Optional support with adjusted layout

## Accessibility

### Color Blindness
- Don't rely solely on color for stat health
- Use icons + shapes + patterns
- Provide alternative theme option

### Text Readability
- Minimum 16px for body text
- High contrast ratios (4.5:1 minimum)
- Avoid text on busy backgrounds

### Reduced Motion
- Option to disable animations
- Instant transitions when enabled
- Stats update immediately without counting

### Language Support
- Text should wrap properly
- Card sizing adjusts to content
- Support for longer languages (German, Spanish)

## UI States Reference

### Stats Display States

**Healthy (60-100):**
- Primary color
- No warnings
- Normal display

**Warning (30-59):**
- Orange color
- Optional: subtle pulse
- Optional: warning icon

**Critical (0-29):**
- Red color
- Persistent pulse
- Warning icon
- Potentially: screen edge glow

### Card Loading States

**Initial Load:**
- Show placeholder/skeleton
- Fade in when ready

**Transition:**
- Previous card exits
- Brief pause (0.1s)
- New card enters

**Error State:**
- Fallback card
- Error icon (subtle)
- "Something went wrong" message

## Performance Guidelines

### Image Loading
- Lazy load card images
- Use atlases for icons
- Compress images (WebP for web, optimized PNG for mobile)
- Max card image size: 800x1066px

### Animation Performance
- Use transform properties (translate, rotate, scale)
- Avoid layout-triggering properties
- Enable hardware acceleration
- Target 60fps on mobile

### Memory Management
- Pool card UI objects
- Unload unused images
- Limit simultaneous animations
- Monitor memory on lower-end devices

## Responsive Design

### Breakpoints (if web)
```
Mobile: 320px - 480px
Tablet: 481px - 768px
Desktop: 769px+
```

### Scaling Strategy
- Cards scale with viewport
- Stats HUD remains proportional
- Text remains readable at all sizes
- Maximum card width: 500px

## Sound Design (Optional)

### Audio Cues
- Card swipe sound (subtle whoosh)
- Decision confirmed (light click)
- Stat change (depending on direction)
- Critical warning (alert tone)
- Victory/defeat (appropriate stinger)

**Volume Guidelines:**
- Default: 50%
- User controllable
- Mute option prominent
- No autoplaying music

## Dark Mode (Future)

**Color Adjustments:**
```
Background: #1E1E1E
Card Base: #2D2D2D
Text Primary: #E0E0E0
Text Secondary: #A0A0A0
Shadows: Lighter, more subtle
Stats: Slightly desaturated colors
```

## Localization Considerations

### Text Space
- Card titles: +30% space buffer
- Situation text: Dynamic height
- Options: Wrap to 2 lines if needed

### RTL Support
- Flip layout for Arabic, Hebrew
- Swipe directions remain physical (left/right)
- Icons mirror appropriately

## Implementation Checklist

- [ ] Stats HUD with 4 stats
- [ ] Card prefab with all elements
- [ ] Swipe gesture detection
- [ ] Drag rotation and highlighting
- [ ] Threshold validation
- [ ] Stat change animations
- [ ] Card pool/recycling system
- [ ] End screen (defeat/victory)
- [ ] Turn counter display
- [ ] Category icon system
- [ ] Color system for stat health
- [ ] Mobile responsive layout
- [ ] Haptic feedback (mobile)
- [ ] Accessibility options
- [ ] Loading/error states

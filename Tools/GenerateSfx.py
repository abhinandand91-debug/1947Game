#!/usr/bin/env python3
# Author:  Abhinandan Dharmadhikari
# Created: 2026-10-16
"""
Synthesises the game's sound effects (no external assets, deterministic).

Usage:  python3 Tools/GenerateSfx.py [output_dir]
Default output: Assets/Resources/Sfx

Built from a few primitives: oscillators with pitch/amplitude curves, inharmonic "bell" partials and
filtered noise. Replace any .wav with a real recording of the same name to swap a sound.
"""
import math, os, random, struct, sys, wave

SR = 44100
TAU = 2 * math.pi
E = math.exp
random.seed(1947)


def n_samples(seconds):
    return int(SR * seconds)


def osc(duration, freq, amp, shape="sin"):
    """Oscillator with phase accumulation. freq/amp are functions of time (seconds)."""
    out, phase = [], 0.0
    for i in range(n_samples(duration)):
        t = i / SR
        phase += TAU * freq(t) / SR
        if shape == "sin":
            v = math.sin(phase)
        elif shape == "tri":
            v = (2 / math.pi) * math.asin(math.sin(phase))
        else:
            v = 2 * ((phase / TAU) % 1.0) - 1
        out.append(v * amp(t) * min(1.0, t / 0.002))
    return out


def bell(freq, duration, decay, gain=1.0, partials=((1, 1.0, 1.0), (2.76, 0.5, 0.6), (5.4, 0.25, 0.35), (8.93, 0.12, 0.2))):
    """Inharmonic additive bell: (frequency ratio, amplitude, decay multiplier) per partial."""
    out = []
    for i in range(n_samples(duration)):
        t = i / SR
        v = sum(a * math.sin(TAU * freq * r * t) * math.exp(-t / (decay * d)) for r, a, d in partials)
        out.append(v * gain * min(1.0, t / 0.002))
    return out


def noise(duration, amp, lowpass=None, highpass=False, sweep=None):
    """White noise with an optional one-pole filter (fixed `lowpass` coefficient or a `sweep(t)` function)."""
    out, y = [], 0.0
    for i in range(n_samples(duration)):
        t = i / SR
        x = random.uniform(-1, 1)
        a = sweep(t) if sweep else lowpass
        if a is not None:
            y += a * (x - y)
            v = (x - y) if highpass else y
        else:
            v = x
        out.append(v * amp(t))
    return out


def mix(*parts):
    """parts: (offset_seconds, samples)."""
    length = max(int(SR * off) + len(s) for off, s in parts)
    buf = [0.0] * length
    for off, s in parts:
        base = int(SR * off)
        for i, v in enumerate(s):
            buf[base + i] += v
    return buf


def finish(samples, peak=0.8):
    m = max(1e-9, max(abs(v) for v in samples))
    scale = peak / m
    fade = int(SR * 0.008)
    out = []
    for i, v in enumerate(samples):
        v *= scale
        if i >= len(samples) - fade:
            v *= (len(samples) - i) / fade
        out.append(max(-1.0, min(1.0, v)))
    return out


def save(path, samples):
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(b"".join(struct.pack("<h", int(v * 32767)) for v in samples))


SOUNDS = {
    # soft pop for buttons
    "ui_click": lambda: mix(
        (0, osc(0.08, lambda t: 520 + 520 * E(-t * 55), lambda t: E(-t / 0.02))),
        (0, noise(0.006, lambda t: 0.25, lowpass=0.5)),
    ),
    # tiny tick when a laser hits something
    "laser_hit": lambda: mix(
        (0, noise(0.07, lambda t: E(-t / 0.012), lowpass=0.7, highpass=True)),
        (0, osc(0.07, lambda t: 1100 - 6000 * t, lambda t: 0.5 * E(-t / 0.02))),
    ),
    # enemy ship destroyed: crunchy burst with a low thump
    "explosion_small": lambda: mix(
        (0, noise(0.5, lambda t: E(-t / 0.12), sweep=lambda t: 0.45 * E(-t * 7) + 0.03)),
        (0, osc(0.35, lambda t: 40 + 90 * E(-t * 14), lambda t: 0.9 * E(-t / 0.1))),
    ),
    # player destroyed: longer, deeper, with debris crackle
    "explosion_big": lambda: mix(
        (0, noise(1.0, lambda t: E(-t / 0.3), sweep=lambda t: 0.5 * E(-t * 4) + 0.02)),
        (0, osc(0.8, lambda t: 30 + 70 * E(-t * 8), lambda t: E(-t / 0.28))),
        (0.08, noise(0.7, lambda t: 0.35 * E(-t / 0.25) * (1 if random.random() < 0.3 else 0.2), lowpass=0.6, highpass=True)),
    ),
    # player takes a hit: buzzy descending zap with a crunch
    "player_hit": lambda: mix(
        (0, osc(0.3, lambda t: 240 - 520 * t, lambda t: max(0.0, 1 - t / 0.3) * (0.7 + 0.3 * math.sin(TAU * 40 * t)), shape="saw")),
        (0, noise(0.12, lambda t: 0.6 * E(-t / 0.04), sweep=lambda t: 0.35)),
    ),
    # game over: slow descending minor phrase
    "game_over": lambda: mix(
        (0.00, osc(0.45, lambda t: 392.0, lambda t: 0.6 * E(-t / 0.3), shape="tri")),
        (0.30, osc(0.45, lambda t: 329.6, lambda t: 0.6 * E(-t / 0.3), shape="tri")),
        (0.60, osc(0.45, lambda t: 261.6, lambda t: 0.6 * E(-t / 0.3), shape="tri")),
        (0.90, osc(1.0, lambda t: 196.0, lambda t: 0.7 * E(-t / 0.5), shape="tri")),
        (0.90, osc(1.0, lambda t: 98.0, lambda t: 0.5 * E(-t / 0.5))),
    ),
    # new best score: sparkling arpeggio into a chord
    "new_best": lambda: mix(
        (0.00, bell(1046.5, 0.8, 0.22, 0.8)),
        (0.07, bell(1318.5, 0.8, 0.22, 0.8)),
        (0.14, bell(1568.0, 0.8, 0.22, 0.8)),
        (0.21, bell(2093.0, 0.8, 0.25, 0.8)),
        (0.30, bell(1046.5, 0.9, 0.35, 0.5)),
        (0.30, bell(1318.5, 0.9, 0.35, 0.5)),
        (0.30, bell(1568.0, 0.9, 0.35, 0.5)),
    ),
}


def main():
    root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    out_dir = sys.argv[1] if len(sys.argv) > 1 else os.path.join(root, "Assets", "Resources", "Sfx")
    os.makedirs(out_dir, exist_ok=True)
    for name, build in SOUNDS.items():
        save(os.path.join(out_dir, name + ".wav"), finish(build()))
        print("wrote", name + ".wav")


if __name__ == "__main__":
    main()

from pathlib import Path
from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "src" / "ModularGameOverlay.App" / "Assets"
SIZES = (16, 20, 24, 32, 40, 48, 64, 128, 256)
COLORS = {
    "background": "#12151C",
    "teal": "#36D3B7",
    "amber": "#FFBB5C",
    "white": "#F1F4F9",
}


def scaled(value: float, scale: int) -> int:
    return round(value * scale / 256)


def render(size: int) -> Image.Image:
    supersample = 4
    canvas_size = size * supersample
    image = Image.new("RGBA", (canvas_size, canvas_size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    s = canvas_size

    def box(x0: int, y0: int, x1: int, y1: int, color: str) -> None:
        draw.rectangle(
            tuple(scaled(value, s) for value in (x0, y0, x1, y1)),
            fill=color,
        )

    draw.rounded_rectangle(
        (0, 0, s - 1, s - 1),
        radius=scaled(44, s),
        fill=COLORS["background"],
    )

    box(42, 42, 114, 66, COLORS["teal"])
    box(42, 42, 66, 114, COLORS["teal"])
    box(142, 190, 214, 214, COLORS["teal"])
    box(190, 142, 214, 214, COLORS["teal"])

    box(142, 42, 214, 66, COLORS["amber"])
    box(190, 42, 214, 114, COLORS["amber"])
    box(42, 190, 114, 214, COLORS["amber"])
    box(42, 142, 66, 214, COLORS["amber"])

    line_width = max(scaled(20, s), 1)
    draw.ellipse(
        tuple(scaled(value, s) for value in (82, 82, 174, 174)),
        outline=COLORS["white"],
        width=line_width,
    )
    draw.rounded_rectangle(
        tuple(scaled(value, s) for value in (118, 118, 138, 138)),
        radius=max(scaled(3, s), 1),
        fill=COLORS["white"],
    )

    return image.resize((size, size), Image.Resampling.LANCZOS)


def main() -> None:
    ASSETS.mkdir(parents=True, exist_ok=True)
    images = [render(size) for size in SIZES]
    images[-1].save(ASSETS / "ModularGameOverlay.png")
    images[-1].save(
        ASSETS / "ModularGameOverlay.ico",
        format="ICO",
        sizes=[(size, size) for size in SIZES],
        append_images=images[:-1],
    )


if __name__ == "__main__":
    main()

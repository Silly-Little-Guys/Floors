shader_type canvas_item;
render_mode unshaded;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform float curvature : hint_range(0.0, 0.15) = 0.018;
uniform float scanline_strength : hint_range(0.0, 0.5) = 0.08;
uniform float vignette_strength : hint_range(0.0, 1.0) = 0.18;
uniform float chromatic_aberration : hint_range(0.0, 0.01) = 0.0005;
uniform float brightness : hint_range(0.5, 1.5) = 1.02;

void fragment() {
	vec2 centered_uv = SCREEN_UV * 2.0 - 1.0;
	float radius_squared = dot(centered_uv, centered_uv);
	vec2 warped_uv = centered_uv * (1.0 + curvature * radius_squared);
	warped_uv = warped_uv * 0.5 + 0.5;

	vec3 color = vec3(0.0);

	if (warped_uv.x >= 0.0 && warped_uv.x <= 1.0 && warped_uv.y >= 0.0 && warped_uv.y <= 1.0) {
		vec2 channel_offset = vec2(chromatic_aberration, 0.0) * (0.35 + radius_squared);
		float red = texture(screen_texture, warped_uv + channel_offset).r;
		float green = texture(screen_texture, warped_uv).g;
		float blue = texture(screen_texture, warped_uv - channel_offset).b;
		color = vec3(red, green, blue);

		float scanline = 1.0 - scanline_strength * (0.5 + 0.5 * sin(SCREEN_UV.y / SCREEN_PIXEL_SIZE.y * 3.14159265));
		float vignette = 1.0 - vignette_strength * smoothstep(0.25, 1.35, radius_squared);
		color *= scanline * vignette * brightness;
	}

	COLOR = vec4(color, 1.0);
}

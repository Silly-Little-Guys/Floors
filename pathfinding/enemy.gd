extends CharacterBody2D

const speed = 35;
@export var player: Node2D
@onready var navigationAgent := $NavigationAgent2D as NavigationAgent2D

func _physics_process(_delta: float) -> void:
	var dir = to_local(navigationAgent.get_next_path_position()).normalized()
	velocity = dir*speed
	move_and_slide()
	
func makePath() -> void:
	navigationAgent.target_position = player.global_position


func _on_timer_timeout():
	makePath()

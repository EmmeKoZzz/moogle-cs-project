#!/bin/bash

run(){
	cd .. || exit
	dotnet watch run --project MoogleServer
	clear
	cd script || exit
}

compile_tex(){
    cd "../$1" || exit
    latexmk -pdf "$1.tex" || exit
    cd "../$1" || exit
}

show_pdf(){
   cd "../$1" || exit
	if [ ! -f "../$1/$1.pdf" ]; then
		compile_tex "$1"
	fi
	
	if [ "$2" = "" ]; then
		xdg-open "$1.pdf"
		else
		$2 "$1.pdf"
	fi
	 
}

clean(){
	clean_tex_aux(){
		# clean report aux files
		mv "../$1/$1.tex" "../"
		rm -rf "../$1"
		mkdir "../$1"
		mv "../$1.tex" "../$1"
	}

	clean_debug_files(){
		# clean debbug files
		rm -rf "../$1/bin"
		rm -rf "../$1/obj"
	}

	clean_tex_aux informe
	clean_tex_aux presentación
	clean_debug_files MoogleEngine
	clean_debug_files MoogleServer
}

help(){
	echo
	echo "Available Functions:"
	FUCNTIONS="run report slides show_report show_slides clean"
	for funct in $FUCNTIONS; do
		echo "- $funct"
	done
	echo
}

if [ "$1" != "show_report" ] && [ "$1" != "show_slides" ] && [ "$2" != "" ] ; then
	echo "$2 is not a command"
	exit
fi

case $1 in
	"run")
		run
		;;
	"report")
		compile_tex informe
		;;
	"slides")
		compile_tex presentación
		;;
	"show_report")
		show_pdf informe "$2"
		;;
	"show_slides")
		show_pdf informe "$2"
		;;
	"clean")
		clean
		;;
esac
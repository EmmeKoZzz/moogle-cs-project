# Moogle

## Requisitos

- .NET Framework 7

## Ejecución y utilización

Para ejecutar el proyecto, primero agregue los documentos que serán tratados como base de datos para las búsquedas en la carpeta "Content", luego ubique la terminal en la carpeta donde se encuentra el proyecto y ejecute el comando - `make dev` - si se encuentra en linux; si lo quiere ejecutar en windows entonces haga el mismo proceso y ejecute el comando - `dotnet watch run --project MoogleServer` -.

Una vez el proyecto ejecutado, si el navegador no se abre, entonces, puede acceder a él mediante la dirección que se muestra en la terminal. Una vez en el navegador, solo necesita escribir, en la entrada de texto que se le muestra, la búsqueda que desee realizar entre los documentos antes mencionados.

## Características

---

### Operadores

El buscador soporta varios operadores:

- **Operador de importancia (*)**:
Se escribe justo delante de la palabra que a la que se le quiere otorgar importancia en la búsqueda. A mayor relevancia tenga esta palabra en el documento más relevante este será.
- **Operador de necesidad (^) y de no-aparición (!)**:
Se escribe justo delante de la palabra y, en dependencia del operador, indica si esa palabra, tiene que aparecer en los documentos mostrados, en caso del operador de necesidad, o que la palabra no puede aparecer en los resultados en caso del operador de no-aparición.
- **Operador de cercanía(~)**:
Se escribe entre las palabras que se desea que estén lo más cercanas posible. A mayor cercanía entre estas palabras más relevante será el documento.

### Funcionalidades

- El buscador te dará sugerencias que te mostrarán más resultados o que te ayudarán a corregir algún error en la consulta.

- En los resultados te mostrará una porción del documento donde se intentará mostrar la parte del documento con mayor relevancia en orden de mejorar su búsqueda

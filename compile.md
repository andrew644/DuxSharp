```bash
#output llvm_ir from .c file
clang -S -emit-llvm foo.c

 #output asm from llmv_ir
llc foo.ll

#make binary from llvm_ir
clang -o foo foo.ll
```

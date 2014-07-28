
#if defined(SGEN_SIMPLE_NURSERY)
#define SERIAL_SCAN_OBJECT_ALLOC simple_nursery_serial_scan_object_alloc

#endif


#undef HANDLE_PTR
/* Global remsets are handled in SERIAL_COPY_OBJECT_FROM_OBJ */
#define HANDLE_PTR(ptr,obj)	do {	\
        MonoObject* mo = (MonoObject*)*ptr;\
        int cocount = mono_gc_collection_count(0);\
        SGEN_LOGT(0,"scanning %s %p gen %d",\
                sgen_safe_name(mo), mo, mo->tenure_gen);\
        /*we only scan if we are fast path alloced*/ \
        if(mo &&    \
            sgen_ptr_in_nursery(mo) && \
            mo->tenure_gen == 0)   \
        {   \
            /*set the nursery tenure if we missed it in alloc*/ \
            mo->tenure_gen =  (cocount+1)*-1; \
            SGEN_LOGT(0,"Setting nursery gen for %s %p gen %d",\
                    sgen_safe_name(mo), mo, mo->tenure_gen);\
        }   \
	} while (0)

static void
SERIAL_SCAN_OBJECT_ALLOC (char *start)
{
//	SGEN_OBJECT_LAYOUT_STATISTICS_DECLARE_BITMAP;

#define SCAN_OBJECT_PROTOCOL
#include "sgen-scan-object.h"

//	SGEN_OBJECT_LAYOUT_STATISTICS_COMMIT_BITMAP;
//	HEAVY_STAT (++stat_scan_object_called_nursery);
}


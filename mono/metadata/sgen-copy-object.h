/*
 * sgen-copy-object.h: This is where objects are copied.
 *
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
 * Copyright (C) 2012 Xamarin Inc
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License 2.0 as published by the Free Software Foundation;
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License 2.0 along with this library; if not, write to the Free
 * Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */
extern long long stat_copy_object_called_nursery;
extern long long stat_objects_copied_nursery;

extern long long stat_nursery_copy_object_failed_from_space;
extern long long stat_nursery_copy_object_failed_forwarded;
extern long long stat_nursery_copy_object_failed_pinned;

extern long long stat_slots_allocated_in_vain;
extern int mono_gc_collection_count(int gen);
extern void print_trace(void);


#define GET_OBJECT_AGE(age,obj) \
    do{\
        age = mono_gc_collection_count(0) - \
            (obj->tenure_gen + 1); \
    }while(0)

static inline void
set_tenure(MonoObject*src,MonoObject*mo)
{
    int age;
    //evacuating to the major
    //we must set the age
    if(src &&
            mo &&
        !sgen_ptr_in_nursery(mo) &&
        sgen_ptr_in_nursery(src))
    {
        /*
         * An object will only be tenured when it is evacuated. Unless the oject
         * is pinned, it will never exist in the nursery longer than one minor collection
         * if it is still alive.
         */
        GET_OBJECT_AGE(age,src);
        //the tenure will be an offset, or the current age minus
        //the absolute value should be used next time it is computed
        mo->tenure_gen = mono_gc_collection_count(1) - age;

        //we were allocated by the managed allocator
        //our birth didn't get set. Luckily this will only make us off by one
        if(src->tenure_gen == 0)
        {
            //decrement
            mo->tenure_gen = (mo->tenure_gen-2)*-1;
            SGEN_LOGT(0,"%s %p it is zero now %d",mo->vtable->klass->name,mo,mo->tenure_gen);
        }
    }
    //we are moving to a different location in the nursery
    //*during compaction
    else if(
            src &&
            mo &&
        sgen_ptr_in_nursery(mo) &&
        sgen_ptr_in_nursery(src) &&
        src->tenure_gen == 0)
    {
        GET_OBJECT_AGE(age,src);
        //the tenure will be an offset, or the current age minus
        //the absolute value should be used next time it is computed
        //the additional decrement will account for the first skipped due to
        //fast path
        mo->tenure_gen = ((mono_gc_collection_count(1) - age)-2)*-1;

        SGEN_LOGT(0,
" tlab birth with %s move to nursery at g0=%d g1=%d src(%p) = %d dest(%p) = %d",
                src->vtable->klass->name,
                mono_gc_collection_count(0),
                mono_gc_collection_count(1),
                src,
                src->tenure_gen,
                mo,
                mo->tenure_gen
                );
    }
    else{
        SGEN_LOGT(0,
                "Null src(%p) or dest(%p) or compacting",
                src,
                mo
                );
    }
}

/*
 * This function can be used even if the vtable of obj is not valid
 * anymore, which is the case in the parallel collector.
 */
static inline void
par_copy_object_no_checks (char *destination, MonoVTable *vt, void *obj, mword objsize, SgenGrayQueue *queue)
{
    MonoObject*mo;
#ifdef __GNUC__
	static const void *copy_labels [] = { &&LAB_0, &&LAB_1, &&LAB_2, &&LAB_3, &&LAB_4, &&LAB_5, &&LAB_6, &&LAB_7, &&LAB_8 };
#endif

	SGEN_ASSERT (9, vt->klass->inited, "vtable %p for class %s:%s was not initialized", vt, vt->klass->name_space, vt->klass->name);
	SGEN_LOG (9, " (to %p, %s size: %lu)", destination, ((MonoObject*)obj)->vtable->klass->name, (unsigned long)objsize);
	binary_protocol_copy (obj, destination, vt, objsize);

    SGEN_LOGT(9," moving object (%s) %p to %p ",sgen_safe_name(obj),obj,destination);
    mo = (MonoObject*)destination;
    //if we are moving from the nursery to the next gen, set the tenure age
    set_tenure(
            (MonoObject*)obj,
            mo);

#ifdef ENABLE_DTRACE
	if (G_UNLIKELY (MONO_GC_OBJ_MOVED_ENABLED ())) {
		int dest_gen = sgen_ptr_in_nursery (destination) ? GENERATION_NURSERY : GENERATION_OLD;
		int src_gen = sgen_ptr_in_nursery (obj) ? GENERATION_NURSERY : GENERATION_OLD;
		MONO_GC_OBJ_MOVED ((mword)destination, (mword)obj, dest_gen, src_gen, objsize, vt->klass->name_space, vt->klass->name);
	}
#endif

#ifdef __GNUC__
	if (objsize <= sizeof (gpointer) * 8) {
		mword *dest = (mword*)destination;
		goto *copy_labels [objsize / sizeof (gpointer)];
	LAB_8:
		(dest) [7] = ((mword*)obj) [7];
	LAB_7:
		(dest) [6] = ((mword*)obj) [6];
	LAB_6:
		(dest) [5] = ((mword*)obj) [5];
	LAB_5:
		(dest) [4] = ((mword*)obj) [4];
	LAB_4:
		(dest) [3] = ((mword*)obj) [3];
	LAB_3:
		(dest) [2] = ((mword*)obj) [2];
	LAB_2:
		(dest) [1] = ((mword*)obj) [1];
	LAB_1:
		;
	LAB_0:
		;
	} else {
		/*can't trust memcpy doing word copies */
		mono_gc_memmove_aligned (destination + sizeof (mword), (char*)obj + sizeof (mword), objsize - sizeof (mword));
	}
#else
		mono_gc_memmove_aligned (destination + sizeof (mword), (char*)obj + sizeof (mword), objsize - sizeof (mword));
#endif
	/* adjust array->bounds */
	SGEN_ASSERT (9, vt->gc_descr, "vtable %p for class %s:%s has no gc descriptor", vt, vt->klass->name_space, vt->klass->name);

	if (G_UNLIKELY (vt->rank && ((MonoArray*)obj)->bounds)) {
		MonoArray *array = (MonoArray*)destination;
		array->bounds = (MonoArrayBounds*)((char*)destination + ((char*)((MonoArray*)obj)->bounds - (char*)obj));
		SGEN_LOG (9, "Array instance %p: size: %lu, rank: %d, length: %lu", array, (unsigned long)objsize, vt->rank, (unsigned long)mono_array_length (array));
	}
	if (G_UNLIKELY (mono_profiler_events & MONO_PROFILE_GC_MOVES))
		sgen_register_moved_object (obj, destination);
	obj = destination;
	if (queue) {
		SGEN_LOG (9, "Enqueuing gray object %p (%s)", obj, sgen_safe_name (obj));
		GRAY_OBJECT_ENQUEUE (queue, obj);
	}
}

/*
 * This can return OBJ itself on OOM.
 */
#ifdef _MSC_VER
static __declspec(noinline) void*
#else
static G_GNUC_UNUSED void* __attribute__((noinline))
#endif
copy_object_no_checks (void *obj, SgenGrayQueue *queue)
{
	MonoVTable *vt = ((MonoObject*)obj)->vtable;
	gboolean has_references = SGEN_VTABLE_HAS_REFERENCES (vt);
	mword objsize = SGEN_ALIGN_UP (sgen_par_object_get_size (vt, (MonoObject*)obj));
	char *destination = COLLECTOR_SERIAL_ALLOC_FOR_PROMOTION (vt, obj, objsize, has_references);
    SGEN_COND_LOGT(0,
            (objsize >= 1512 && objsize < 2500) ||
            strcmp("BigObject",vt->klass->name) == 0,
            "promoting %s %p dest:%p",
            vt->klass->name,
            obj,
            destination
            );
    //if we are moving from the nursery to the next gen, set the tenure age
    set_tenure(
            (MonoObject*)obj,
            (MonoObject*)destination);

	if (G_UNLIKELY (!destination)) {
		collector_pin_object (obj, queue);
		sgen_set_pinned_from_failed_allocation (objsize);
		return obj;
	}

	par_copy_object_no_checks (destination, vt, obj, objsize, has_references ? queue : NULL);
	/* FIXME: mark mod union cards if necessary */

	/* set the forwarding pointer */
	SGEN_FORWARD_OBJECT (obj, destination);

	return destination;
}

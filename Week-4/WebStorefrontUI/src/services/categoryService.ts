import { api } from "./api";
import type { Category } from "../types/Category";

export const categoryService = {

    // First up, the GET to get all categories in the db
    async getAllCategories(): Promise<Category[]> {
        // Use our premade api object, to call the /api/Category GET
        const response = await api.get<Category[]>('/Category');
        return response.data;
    },

    // Add the DELETE by Id call 
    async deleteById(id: number): Promise<void> {
        await api.delete(`/Category/${id}`)
    }

};